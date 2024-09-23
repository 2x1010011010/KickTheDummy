using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FluidFlow
{
    [System.Serializable]
    public struct StitchData
    {
        public Mesh Mesh;
        public UVSet UVSet;
        public StitchResult[] Stitches;

        public StitchData(Mesh mesh, UVSet uVSet, StitchResult[] stitches)
        {
            Mesh = mesh;
            UVSet = uVSet;
            Stitches = stitches;
        }
    }

    [System.Serializable]
    public struct StitchResult
    {
        public int SubmeshIndex;
        public Stitcher.Stitch[] InternalStitches;
        public Stitcher.Edge[] OuterEdges;

        public StitchResult(int submeshIndex, Stitcher.Stitch[] internalStitches, Stitcher.Edge[] outerEdges)
        {
            SubmeshIndex = submeshIndex;
            InternalStitches = internalStitches;
            OuterEdges = outerEdges;
        }
    }

    public static class Stitcher
    {
        public static GenerationHandle Generate(Mesh mesh, UVSet uvSet)
        {
            return new GenerationHandle(mesh, uvSet);
        }

        [System.Serializable]
        public struct Stitch
        {
            public float2 A0, B0, A1, B1;
            public float N0, N1;

            private static float ToPolar(float2 direction)
            {
                return (math.atan2(direction.y, direction.x) + math.PI * 2) % (math.PI * 2);
            }

            public Stitch(in Edge e0, in Edge e1)
            {
                A0 = e0.UvA;
                B0 = e0.UvB;
                N0 = ToPolar(e0.Normal());
                A1 = e1.UvA;
                B1 = e1.UvB;
                N1 = ToPolar(e1.Normal());
            }

            public Stitch(float2 a0, float2 b0, float2 a1, float2 b1, float n0, float n1)
            {
                A0 = a0;
                B0 = b0;
                A1 = a1;
                B1 = b1;
                N0 = n0;
                N1 = n1;
            }

            public Stitch Transform(in float4 atlasTransform)
            {
                return new Stitch(
                    A0 * atlasTransform.xy + atlasTransform.zw,
                    B0 * atlasTransform.xy + atlasTransform.zw,
                    A1 * atlasTransform.xy + atlasTransform.zw,
                    B1 * atlasTransform.xy + atlasTransform.zw,
                    N0,
                    N1);
            }
        }

        public static int CompareVertices(float3 a, float3 b)
        {
            if (a.x < b.x)
                return 1;
            if (a.x == b.x) {
                if (a.y < b.y)
                    return 1;
                if (a.y == b.y) {
                    if (a.z < b.z)
                        return 1;
                    if (a.z == b.z)
                        return 0;
                }
            }
            return -1;
        }
        private struct EdgeComparer : IComparer<Edge>
        {
            public int Compare(Edge a, Edge b) => CompareVertices(a.VertA, b.VertA);
        }

        [System.Serializable]
        public struct Edge
        {
            public float3 VertA, VertB;
            public float2 UvA, UvB, UvC;

            public Edge(float3 vertA, float3 vertB, float2 uvA, float2 uvB, float2 uvC)
            {
                var switchAB = CompareVertices(vertA, vertB) < 0;
                VertA = switchAB ? vertB : vertA;
                VertB = switchAB ? vertA : vertB;
                UvA = switchAB ? uvB : uvA;
                UvB = switchAB ? uvA : uvB;
                UvC = uvC;
            }

            public float2 Normal()
            {
                var ab = UvB - UvA;
                var f = UvA + ab * (math.dot(UvC - UvA, ab) / math.lengthsq(ab));
                return math.normalize(UvC - f);
            }

            public Edge Transform(in float4 atlasTransform)
            {
                return new Edge(
                    VertA,
                    VertB,
                    UvA * atlasTransform.xy + atlasTransform.zw,
                    UvB * atlasTransform.xy + atlasTransform.zw,
                    UvC * atlasTransform.xy + atlasTransform.zw);
            }
        }

        public class GenerationHandle : System.IDisposable
        {
            private struct Result
            {
                public int SubmeshIndex;
                public NativeArray<Stitch> InternalStitches;    // connected uv-seams within a submesh
                public NativeArray<Edge> OuterEdges;    // unconnected edges, at the border of the submesh uv map
                public NativeArray<int> Counts;
            }

            public readonly Mesh SourceMesh;
            public readonly UVSet UVSet;
            private GetEdgesHandle GetEdgesHandle;
            private readonly Result[] GenerationResults;
            private NativeArray<JobHandle> Handles;
            public JobHandle Handle { get; private set; }

            public GenerationHandle(Mesh from, UVSet uvSet)
            {
                SourceMesh = from;
                UVSet = uvSet;
                GetEdgesHandle = new GetEdgesHandle(from, uvSet, (int)(1u << from.subMeshCount) - 1);
                GenerationResults = new Result[GetEdgesHandle.GetEdgesJobs.Length];
                Handles = new NativeArray<JobHandle>(GenerationResults.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (var i = 0; i < GenerationResults.Length; i++) {
                    var handle = GetEdgesHandle.GetEdgesJobs[i].Schedule();
                    GenerationResults[i] = new Result() {
                        SubmeshIndex = GetEdgesHandle.GetEdgesJobs[i].SubmeshIndex,
                        InternalStitches = new NativeArray<Stitch>(GetEdgesHandle.GetEdgesJobs[i].SubmeshCount / 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                        OuterEdges = new NativeArray<Edge>(GetEdgesHandle.GetEdgesJobs[i].SubmeshCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                        Counts = new NativeArray<int>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory)
                    };
                    handle = GetEdgesHandle.GetEdgesJobs[i].Edges.SortJob(new EdgeComparer()).Schedule(handle); // sweep & prune, to find seams in O(n log n), instead of O(n^2)
                    Handles[i] = new GetStitchesJob() {
                        Edges = GetEdgesHandle.GetEdgesJobs[i].Edges,
                        Stitches = GenerationResults[i].InternalStitches,
                        OuterEdges = GenerationResults[i].OuterEdges,
                        StitchCount = GenerationResults[i].Counts
                    }.Schedule(handle);
                }
                Handle = JobHandle.CombineDependencies(Handles);
            }

            public int StitchCount(int submeshMask)
            {
                var count = 0;
                for (var i = 0; i < GenerationResults.Length; i++)
                    if (submeshMask.IsBitSet(GenerationResults[i].SubmeshIndex))
                        count += GenerationResults[i].Counts[0];
                return count;
            }

            public int EdgesCount(int submeshMask)
            {
                var count = 0;
                for (var i = 0; i < GenerationResults.Length; i++)
                    if (submeshMask.IsBitSet(GenerationResults[i].SubmeshIndex))
                        count += GenerationResults[i].Counts[1];
                return count;
            }

            public StitchData Get()
            {
                var result = new StitchData() {
                    Mesh = SourceMesh,
                    UVSet = UVSet,
                    Stitches = new StitchResult[GenerationResults.Length],
                };
                for (var i = 0; i < GenerationResults.Length; i++) {
                    var gen = GenerationResults[i];
                    result.Stitches[i] = new StitchResult(
                        gen.SubmeshIndex,
                        gen.InternalStitches.GetSubArray(0, gen.Counts[0]).ToArray(),
                        gen.OuterEdges.GetSubArray(0, gen.Counts[1]).ToArray());
                }
                return result;
            }

            public void Dispose()
            {
                Handle.Complete();
                GetEdgesHandle.Dispose();
                Handles.Dispose();
                for (var i = 0; i < GenerationResults.Length; i++) {
                    GenerationResults[i].InternalStitches.Dispose();
                    GenerationResults[i].OuterEdges.Dispose();
                    GenerationResults[i].Counts.Dispose();
                }
            }
        }

        private struct GetEdgesHandle : System.IDisposable
        {
            private NativeArray<float3> Vertices;
            private NativeArray<float2> UVs;
            private NativeArray<int> Triangles;
            public GetEdgesJob[] GetEdgesJobs { get; private set; }

            public GetEdgesHandle(Mesh mesh, UVSet uvSet, int submesh)
            {
                var validatedMask = mesh.ValidateSubmeshMask(submesh);
                var totalIndexCount = 0;
                {
                    var submeshCount = 0;
                    for (var it = validatedMask.IterateFlags(); it.Valid(); it.Next()) {
                        totalIndexCount += mesh.GetSubMesh(it.Index()).indexCount;
                        submeshCount++;
                    }
                    GetEdgesJobs = new GetEdgesJob[submeshCount];
                }
                using (var data = Mesh.AcquireReadOnlyMeshData(mesh)) {
                    Vertices = new NativeArray<float3>(data[0].vertexCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                    data[0].GetVertices(Vertices.Reinterpret<Vector3>());
                    // TODO: merge vertices by distance as preprocessing step (maybe)
                    UVs = new NativeArray<float2>(data[0].vertexCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                    data[0].GetUVs(uvSet == UVSet.UV0 ? 0 : 1, UVs.Reinterpret<Vector2>());

                    Triangles = new NativeArray<int>(totalIndexCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                    var index = 0;
                    var start = 0;

                    for (var it = validatedMask.IterateFlags(); it.Valid(); it.Next()) {
                        var count = data[0].GetSubMesh(it.Index()).indexCount;
                        data[0].GetIndices(Triangles.GetSubArray(start, count), it.Index());
                        GetEdgesJobs[index] = new GetEdgesJob(Vertices, UVs, Triangles, it.Index(), start, count);
                        start += count;
                        index++;
                    }
                }
            }

            public void Dispose()
            {
                Vertices.Dispose();
                UVs.Dispose();
                Triangles.Dispose();
                for (var i = GetEdgesJobs.Length - 1; i >= 0; i--)
                    GetEdgesJobs[i].Dispose();
            }
        }

        [BurstCompile]
        private struct GetEdgesJob : IJobParallelFor, System.IDisposable
        {
            public readonly int SubmeshIndex;
            public readonly int SubmeshStart;
            public readonly int SubmeshCount;

            [ReadOnly] private NativeArray<float3> Vertices;
            [ReadOnly] private NativeArray<float2> UVs;
            [ReadOnly] private NativeArray<int> Triangles;
            [WriteOnly] public NativeArray<Edge> Edges;

            public GetEdgesJob(NativeArray<float3> vertices, NativeArray<float2> uvs, NativeArray<int> triangles, int submeshIndex, int start, int count)
            {
                Vertices = vertices;
                UVs = uvs;
                Triangles = triangles;
                SubmeshIndex = submeshIndex;
                SubmeshStart = start;
                SubmeshCount = count;
                Edges = new NativeArray<Edge>(count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }

            public JobHandle Schedule()
            {
                return this.Schedule(SubmeshCount, 2048);   // TODO: expose batch size
            }

            public void Execute(int i)
            {
                var t = ((SubmeshStart + i) / 3) * 3;
                var a = Triangles[t + ((i + 0) % 3)];
                var b = Triangles[t + ((i + 1) % 3)];
                var c = Triangles[t + ((i + 2) % 3)];
                Edges[i] = new Edge(Vertices[a], Vertices[b], UVs[a], UVs[b], UVs[c]);
            }

            public void Dispose()
            {
                Edges.Dispose();
            }
        }

        [BurstCompile]
        private struct GetStitchesJob : IJob
        {
            public NativeArray<Edge> Edges;
            [WriteOnly] public NativeArray<Stitch> Stitches;
            [WriteOnly] public NativeArray<Edge> OuterEdges;
            [WriteOnly] public NativeArray<int> StitchCount;

            public void Execute()
            {
                var stitchCount = 0;
                var outerCount = 0;
                for (int a = 0; a < Edges.Length; a++) {
                    var edgeA = Edges[a];
                    if (math.isnan(edgeA.VertA.x))
                        continue;
                    for (int b = a + 1; b < Edges.Length; b++) {
                        var edgeB = Edges[b];
                        var dA = math.distancesq(edgeA.VertA, edgeB.VertA);
                        var dB = math.distancesq(edgeA.VertB, edgeB.VertB);

                        if (dA > 0)
                        // if (edgeA.VertA.x != edgeB.VertA.x || edgeA.VertA.y != edgeB.VertA.y || edgeA.VertA.z != edgeB.VertA.z)
                        {
                            // edge A has not been matched -> outer edge
                            OuterEdges[outerCount++] = edgeA;
                            break;
                        } else if (dB == 0)
                          // else if (edgeA.VertB.x == edgeB.VertB.x && edgeA.VertB.y == edgeB.VertB.y && edgeA.VertB.z == edgeB.VertB.z)
                          {
                            if (math.distancesq(edgeA.UvA, edgeB.UvA) > 0 || math.distancesq(edgeA.UvB, edgeB.UvB) > 0) {
                                // vertex positions match, but at least one UV does not -> seam
                                Stitches[stitchCount++] = new Stitch(edgeA, edgeB);
                            } else {
                                //  vertices and uvs match -> inner edge 
                                // -> mark Edges[b] as matched
                                Edges[b] = new Edge(math.NAN, math.NAN, math.NAN, math.NAN, math.NAN);
                            }
                            break;
                        } else {
                            // vertex a matches, but b does not -> try next b
                        }
                    }
                }
                StitchCount[0] = stitchCount;
                StitchCount[1] = outerCount;
            }
        }
    }
}
