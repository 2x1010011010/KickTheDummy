using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class Gravity
    {
#if UNITY_EDITOR
        public static Mesh GenerateSecondaryUVCache(Mesh source, UnityEditor.UnwrapParam unwrapParam)
        {
            using (var generation = new GenerationHandle(source, Utility.CopyMesh(source), unwrapParam)) {
                generation.Handle.Complete();
                generation.Apply();
                return generation.Mesh;
            }
        }
#endif

        public class GenerationHandle : System.IDisposable
        {
            public readonly Mesh Source;
            public readonly Mesh Mesh;
            private CreateSecondaryUVTransformJob Job;
            public JobHandle Handle { get; private set; }

            public GenerationHandle(Mesh sourceMesh, Mesh mesh)
            {
                Source = sourceMesh;
                Mesh = mesh;
                Job = new CreateSecondaryUVTransformJob(Mesh);
                Handle = Job.Schedule();
            }

#if UNITY_EDITOR
            public GenerationHandle(Mesh sourceMesh, Mesh mesh, UnityEditor.UnwrapParam unwrapParam)
            {
                Source = sourceMesh;
                Mesh = mesh;
                if (!Mesh.HasVertexAttribute(VertexAttribute.TexCoord1))
                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(Mesh, unwrapParam);
                Job = new CreateSecondaryUVTransformJob(Mesh);
                Handle = Job.Schedule();
            }
#endif

            public void Apply()
            {
                var unusedStream = Mesh.vertexBufferCount;
                if (unusedStream < 4 && SystemInfo.SupportsVertexAttributeFormat(VertexAttributeFormat.Float16, 4)) {
                    var vertexAttributes = new List<VertexAttributeDescriptor>();
                    Mesh.GetVertexAttributes(vertexAttributes);
                    var index = vertexAttributes.FindIndex(descr => descr.attribute == VertexAttribute.TexCoord2);
                    if (index != -1) {
                        Debug.LogError("FluidFlow: {0} already contains texcoord2!", Mesh);
                        return;
                    }
                    index = vertexAttributes.FindLastIndex(descr => (int)descr.attribute < (int)VertexAttribute.TexCoord2);
                    vertexAttributes.Insert(index + 1, new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float16, 4, unusedStream));
                    Mesh.SetVertexBufferParams(Mesh.vertexCount, vertexAttributes.ToArray());
                    Mesh.SetVertexBufferData(Job.Transformations, 0, 0, Job.Transformations.Length, unusedStream);
                } else {
                    Mesh.SetUVs(2, Job.Transformations);
                }
            }

            public void Dispose()
            {
                Handle.Complete();
                Job.Dispose();
            }
        }

        public struct CreateSecondaryUVTransformJob : IJob, System.IDisposable
        {
            [ReadOnly] private NativeArray<int> Triangles;
            [ReadOnly] private NativeArray<float3> Vertices;
            [ReadOnly] private NativeArray<float3> Normals;
            [ReadOnly] private NativeArray<float4> Tangents;
            [ReadOnly] private NativeArray<float2> UVs;
            [WriteOnly] public NativeArray<half4> Transformations;
            private NativeArray<float3> tmpTangents;
            private NativeArray<float3> tmpBiTangents;

            public CreateSecondaryUVTransformJob(Mesh mesh)
            {
                using (var data = Mesh.AcquireReadOnlyMeshData(mesh)) {
                    var vertexCount = data[0].vertexCount;
                    var triangleCount = data[0].GetTriangleCount();
                    Triangles = new NativeArray<int>(triangleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    data[0].GetTriangles(Triangles);
                    Vertices = new NativeArray<float3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    data[0].GetVertices(Vertices.Reinterpret<Vector3>());
                    Normals = new NativeArray<float3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    data[0].GetNormals(Normals.Reinterpret<Vector3>());
                    Tangents = new NativeArray<float4>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    data[0].GetTangents(Tangents.Reinterpret<Vector4>());
                    UVs = new NativeArray<float2>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    data[0].GetUVs(1, UVs.Reinterpret<Vector2>());
                    tmpTangents = new NativeArray<float3>(vertexCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                    tmpBiTangents = new NativeArray<float3>(vertexCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                    Transformations = new NativeArray<half4>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                }
            }

            // calculate conversions for converting between UV0 and UV1 tangent space
            public void Execute()
            {
                for (int i = 0; i < Triangles.Length; i += 3) {
                    int i0 = Triangles[i], i1 = Triangles[i + 1], i2 = Triangles[i + 2];
                    var e0uv = UVs[i1] - UVs[i0];
                    var e1uv = UVs[i2] - UVs[i0];
                    var e0vert = Vertices[i1] - Vertices[i0];
                    var e1vert = Vertices[i2] - Vertices[i0];
                    var r0 = 1.0f / (e0uv.x * e1uv.y - e1uv.x * e0uv.y);
                    var tangent = (e0vert * e1uv.y - e1vert * e0uv.y) * r0;
                    var bitangent = (e1vert * e0uv.x - e0vert * e1uv.x) * r0;
                    tmpTangents[i0] += tangent;
                    tmpTangents[i1] += tangent;
                    tmpTangents[i2] += tangent;
                    tmpBiTangents[i0] += bitangent;
                    tmpBiTangents[i1] += bitangent;
                    tmpBiTangents[i2] += bitangent;
                }
                for (int i = 0; i < Vertices.Length; i++) {
                    var tangent0 = Tangents[i].xyz;
                    var bitangent0 = math.cross(Normals[i], tangent0) * Tangents[i].w;
                    var orthonorm = math.orthonormalize(new float3x3(Normals[i], tmpTangents[i], tmpBiTangents[i]));
                    Transformations[i] = math.half4(new float4(
                        math.dot(tangent0, orthonorm.c1),
                        math.dot(bitangent0, orthonorm.c1),
                        math.dot(tangent0, orthonorm.c2),
                        math.dot(bitangent0, orthonorm.c2)));
                }
            }

            public void Dispose()
            {
                Transformations.Dispose();
                Triangles.Dispose();
                Vertices.Dispose();
                Normals.Dispose();
                Tangents.Dispose();
                UVs.Dispose();
                tmpTangents.Dispose();
                tmpBiTangents.Dispose();
            }
        }
    }
}