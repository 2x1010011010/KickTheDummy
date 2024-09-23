using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FluidFlow
{
    public static class StitchMap
    {
        public const int MAX_STITCH_RESOLUTION = 8192;
        public static readonly MaterialCache FluidUVSeamStitch = new MaterialCache(InternalShaders.RootPath + "/Fluid/UVSeamStitch", FFFlowTextureUtil.SetFlowTexCompressed);
        public static readonly MaterialCache FluidUVSeamPadding = new MaterialCache(InternalShaders.RootPath + "/Fluid/UVSeamPadding");

        public static void Draw(List<Surface> surfaces, RenderTexture target)
        {
            var stitchData = new StitchResult[surfaces.Count][];
            var maxStitchCount = 0;
            for (var i = 0; i < surfaces.Count; i++) {
                if (!Cache.TryGetStitches(surfaces[i].Mesh, surfaces[i].UVSet, out stitchData[i])) {
                    Debug.LogErrorFormat("FluidFlow: Stitch generation failed {0} {1}. No stitch data available.", surfaces[i].Mesh, surfaces[i].UVSet);
                    return;
                }
                foreach (var submeshIndex in surfaces[i].CombinedSubmeshMask().EnumerateSetBits()) {
                    maxStitchCount += stitchData[i][submeshIndex].InternalStitches.Length;
                    maxStitchCount += stitchData[i][submeshIndex].OuterEdges.Length;
                }
            }

            var index = 0;
            var stitches = new NativeArray<Stitcher.Stitch>(maxStitchCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var edges = new List<System.Tuple<Vector4, Stitcher.Edge[]>>();
            for (var i = 0; i < surfaces.Count; i++) {
                var data = stitchData[i];
                foreach (var submesh in surfaces[i].EnumerateSubmeshes()) {
                    var r = 0;  // find matching result handle for submesh
                    for (; r < data.Length; r++)
                        if (data[r].SubmeshIndex == submesh.Index)
                            break;
                    // add internal stitches
                    var resultStitches = data[r].InternalStitches;
                    for (var n = 0; n < resultStitches.Length; n++)
                        stitches[index++] = resultStitches[n].Transform(submesh.AtlasTransform);

                    // try connect outer edges
                    var edgesA = data[r].OuterEdges;
                    for (var e = 0; e < edges.Count; e++) {
                        var edgesB = edges[e].Item2;

                        var indexA = 0;
                        var indexB = 0;
                        while (indexA < edgesA.Length && indexB < edgesB.Length) {
                            var edgeA = edgesA[indexA];
                            var edgeB = edgesB[indexB];

                            if (math.distancesq(edgeA.VertA, edgeB.VertA) == 0 && math.distancesq(edgeA.VertB, edgeB.VertB) == 0) {
                                var transformedA = edgeA.Transform(submesh.AtlasTransform);
                                var transformedB = edgeB.Transform(edges[e].Item1);
                                if (math.distancesq(transformedA.UvA, transformedB.UvA) > 0 || math.distancesq(transformedA.UvB, transformedB.UvB) > 0) {
                                    // vertex positions match, but at least one UV does not -> seam
                                    stitches[index++] = new Stitcher.Stitch(transformedA, transformedB);
                                    indexA++;
                                    indexB++;
                                    continue;
                                }
                            }
                            if (Stitcher.CompareVertices(edgeA.VertA, edgeB.VertA) < 0)
                                indexA++;
                            else
                                indexB++;
                        }
                    }
                    edges.Add(new System.Tuple<Vector4, Stitcher.Edge[]>(submesh.AtlasTransform, edgesA));
                }
            }
            using (var stitchMesh = new StitchMesh(stitches.GetSubArray(0, index))) {
                using (RestoreRenderTarget.RestoreActive()) {
                    using (var tmp = new TmpRenderTexture(target.descriptor)) {
                        Graphics.SetRenderTarget(tmp);
                        GL.Clear(false, true, Color.clear);
                        Shader.SetGlobalVector(InternalShaders.TexelSizePropertyID, target.GetTexelSize());
                        FFFlowTextureUtil.FlowTextureVariant(FluidUVSeamStitch, target).SetPass(0);
                        Graphics.DrawMeshNow(stitchMesh.Mesh, Matrix4x4.identity);  // draw stitch map
                        Graphics.Blit(tmp, target, FluidUVSeamPadding); // add padding
                    }
                }
            }
            stitches.Dispose();
        }

        private struct StitchMesh : System.IDisposable
        {
            public Mesh Mesh { get => mesh; }
            private readonly Mesh mesh;

            public StitchMesh(NativeArray<Stitcher.Stitch> stitches)
            {
                var count = stitches.Length;
                using (var initJob = new InitMesh() {
                    Stitches = stitches,
                    Vertices = new NativeArray<float3>(count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                    Data = new NativeArray<float4>(count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                    Lines = new NativeArray<int>(count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
                }) {
                    initJob.Schedule(count, 512).Complete();
                    mesh = new Mesh();
                    mesh.SetVertices(initJob.Vertices);
                    mesh.SetUVs(0, initJob.Data);
                    mesh.SetIndices(initJob.Lines, MeshTopology.Lines, 0);
                    mesh.UploadMeshData(true);
                }
            }

            public void Dispose()
            {
                Object.Destroy(mesh);
            }
        }

        [BurstCompile]
        private struct InitMesh : IJobParallelFor, System.IDisposable
        {
            [ReadOnly] public NativeArray<Stitcher.Stitch> Stitches;
            [NativeDisableParallelForRestriction] public NativeArray<float3> Vertices;
            [NativeDisableParallelForRestriction] public NativeArray<float4> Data;
            [NativeDisableParallelForRestriction] public NativeArray<int> Lines;

            public void Execute(int index)
            {
                var stitch = Stitches[index];
                var targetIndex = index * 4;
                Vertices[targetIndex + 0] = new float3(stitch.A0, 0);
                Vertices[targetIndex + 1] = new float3(stitch.B0, 0);
                Vertices[targetIndex + 2] = new float3(stitch.A1, 0);
                Vertices[targetIndex + 3] = new float3(stitch.B1, 0);
                Data[targetIndex + 0] = new float4(stitch.A1.xy, stitch.N0, stitch.N1);
                Data[targetIndex + 1] = new float4(stitch.B1.xy, stitch.N0, stitch.N1);
                Data[targetIndex + 2] = new float4(stitch.A0.xy, stitch.N1, stitch.N0);
                Data[targetIndex + 3] = new float4(stitch.B0.xy, stitch.N1, stitch.N0);
                Lines[targetIndex + 0] = targetIndex + 0;
                Lines[targetIndex + 1] = targetIndex + 1;
                Lines[targetIndex + 2] = targetIndex + 2;
                Lines[targetIndex + 3] = targetIndex + 3;
            }

            public void Dispose()
            {
                Vertices.Dispose();
                Data.Dispose();
                Lines.Dispose();
            }
        }
    }
}