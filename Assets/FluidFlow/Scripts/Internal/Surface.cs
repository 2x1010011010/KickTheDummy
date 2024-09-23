using System;
using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    [Serializable]
    public struct SurfaceDescriptor
    {
        [Serializable]
        public struct SubmeshDescriptor
        {
            public int SubmeshMask;
            public float UVScale;
            public float UVAspect;
            public Vector2 UVOffset;

            public static SubmeshDescriptor Default()
            {
                return new SubmeshDescriptor() {
                    SubmeshMask = ~0,
                    UVScale = 1,
                    UVAspect = 1,
                    UVOffset = Vector2.zero
                };
            }

            public Vector4 AtlasTransform(float textureAspectRatio)
            {
                return new Vector4(UVScale * (UVAspect / textureAspectRatio), UVScale, UVOffset.x, UVOffset.y);
            }

            public void Scale(Vector2 scale)
            {
                UVOffset *= scale;
                UVScale *= scale.y;
            }
        }

        public Renderer Renderer;
        public FFModelCache Cache;
        public UVSet UVSet;
        public List<SubmeshDescriptor> SubmeshDescriptors;

        public static SurfaceDescriptor Default()
        {
            return new SurfaceDescriptor() {
                Renderer = null,
                Cache = null,
                UVSet = UVSet.UV0,
                SubmeshDescriptors = new List<SubmeshDescriptor>() { SubmeshDescriptor.Default() }
            };
        }

        public ValueTuple<Surface, Optional<Gravity.GenerationHandle>> ToSurface(float textureAspectRatio)
        {
            if (!Renderer)
                Debug.LogErrorFormat("FluidFlow: Unable to initialize Surface! No renderer assigned.");
            var mesh = Renderer.GetMesh();
            if (!mesh)
                Debug.LogErrorFormat("FluidFlow: Unable to initialize Surface! Surface renderer '{0}' has no associated mesh.", Renderer);
            if (Cache)
                Cache.Initialize();
            var generationHandle = Optional<Gravity.GenerationHandle>.None;
            if (UVSet == UVSet.UV1 && !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord2)) {
                if (!FluidFlow.Cache.TryGetMesh(mesh, out var secondaryUVMesh)) {
                    generationHandle = FluidFlow.Cache.Request(mesh);
                    if (generationHandle.Enabled)
                        secondaryUVMesh = generationHandle.Value.Mesh;
                }
                if (secondaryUVMesh) {
                    mesh = secondaryUVMesh;
                    Renderer.SetMesh(mesh);
                } else {
                    Debug.LogErrorFormat("FluidFlow: Failed getting secondary UV data for '{0}'. Falling back to UV0.", mesh);
                    UVSet = UVSet.UV0;
                }
            }
            var descriptors = new Surface.SubmeshDescriptor[SubmeshDescriptors.Count];
            for (var i = descriptors.Length - 1; i >= 0; i--)
                descriptors[i] = new Surface.SubmeshDescriptor(mesh, SubmeshDescriptors[i], textureAspectRatio);
            return ValueTuple.Create(new Surface(Renderer, mesh, UVSet, descriptors), generationHandle);
        }
    }

    public readonly struct Surface
    {
        public readonly struct SubmeshDescriptor
        {
            public readonly int SubmeshMask;
            public readonly Vector4 AtlasTransform;

            public SubmeshDescriptor(Mesh mesh, SurfaceDescriptor.SubmeshDescriptor descriptor, float textureAspectRatio)
            {
                SubmeshMask = mesh.ValidateSubmeshMask(descriptor.SubmeshMask);
                AtlasTransform = descriptor.AtlasTransform(textureAspectRatio);
            }
        }

        public readonly Renderer Renderer;
        public readonly Mesh Mesh;
        public readonly UVSet UVSet;
        public readonly SubmeshDescriptor[] SubmeshDescriptors;

        public Surface(Renderer renderer, Mesh mesh, UVSet uVSet, SubmeshDescriptor[] submeshDescriptors)
        {
            Renderer = renderer;
            Mesh = mesh;
            UVSet = uVSet;
            SubmeshDescriptors = submeshDescriptors;
        }

        public int CombinedSubmeshMask()
        {
            int mask = 0;
            for (var i = 0; i < SubmeshDescriptors.Length; i++)
                mask |= SubmeshDescriptors[i].SubmeshMask;
            return mask;
        }

        public readonly struct SubmeshTransformPair
        {
            public readonly int Index;
            public readonly Vector4 AtlasTransform;

            public SubmeshTransformPair(int index, Vector4 atlasTransform)
            {
                Index = index;
                AtlasTransform = atlasTransform;
            }
        }

        public IEnumerable<SubmeshTransformPair> EnumerateSubmeshes()
        {
            for (var index = 0; index < sizeof(int) * 8; index++) {
                for (var i = 0; i < SubmeshDescriptors.Length; i++)
                    if (SubmeshDescriptors[i].SubmeshMask.IsBitSet(index))
                        yield return new SubmeshTransformPair(index, SubmeshDescriptors[i].AtlasTransform);
            }
        }
    }

    public struct SurfaceSubmeshInfo
    {
        public Mesh Mesh;
        public int SubmeshIndex;
        public UVSet UVSet;
        public Vector4 AtlasTransform;

        public SurfaceSubmeshInfo(Mesh mesh, int submeshIndex, UVSet uVSet, Vector4 atlasTransform)
        {
            Mesh = mesh;
            SubmeshIndex = submeshIndex;
            UVSet = uVSet;
            AtlasTransform = atlasTransform;
        }
    }

    public static class SurfaceExtensions
    {
        public static Rect[] UVBounds(Mesh mesh, UVSet uvSet)
        {
            var ranges = new Rect[mesh.subMeshCount];
            var uvs = mesh.GetUVSet(uvSet);
            var indices = new List<int>();
            for (var s = 0; s < mesh.subMeshCount; s++) {
                var min = Vector2.one * float.MaxValue;
                var max = Vector2.one * float.MinValue;
                mesh.GetIndices(indices, s);
                for (var i = indices.Count - 1; i >= 0; i--) {
                    var uv = uvs[indices[i]];
                    min = Vector2.Min(min, uv);
                    max = Vector2.Max(max, uv);
                }
                ranges[s] = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
            }
            return ranges;
        }

        public static Rect Combine(this Rect[] uvBounds, int mask)
        {
            if (mask == 0 || mask >= 1u << uvBounds.Length)
                return new Rect();
            var min = Vector2.one * float.MaxValue;
            var max = Vector2.one * float.MinValue;
            for (var it = mask.IterateFlags(); it.Valid(); it.Next()) {
                min = Vector2.Min(min, uvBounds[it.Index()].min);
                max = Vector2.Max(max, uvBounds[it.Index()].max);
            }
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private static SurfaceDescriptor.SubmeshDescriptor Normalized(Mesh mesh, UVSet uvSet, int submeshMask, float textureAspectRatio = 1)
        {
            submeshMask = mesh.ValidateSubmeshMask(submeshMask);
            var bounds = UVBounds(mesh, uvSet).Combine(submeshMask);
            var scaleInv = 1f / Mathf.Max(bounds.width, bounds.height);
            var descriptor = new SurfaceDescriptor.SubmeshDescriptor() {
                SubmeshMask = submeshMask,
                UVOffset = new Vector2(-bounds.min.x / textureAspectRatio, -bounds.min.y) * scaleInv,
                UVScale = scaleInv,
                UVAspect = 1
            };
            return descriptor;
        }

        public static void Normalize(this SurfaceDescriptor descr, int submeshDescrIndex, float textureAspectRatio = 1)
        {
            if (!descr.Renderer) {
                Debug.LogError("FluidFlow: Failed to normalize surface descriptor! No renderer assigned.");
                return;
            }
            var mesh = descr.Renderer.GetMesh();
            if (!mesh) {
                Debug.LogErrorFormat("FluidFlow: Failed to normalize surface descriptor! No mesh assigned to renderer {0}.", descr.Renderer);
                return;
            }
            descr.SubmeshDescriptors[submeshDescrIndex] = Normalized(mesh, descr.UVSet, descr.SubmeshDescriptors[submeshDescrIndex].SubmeshMask, textureAspectRatio);
        }

        public static void GridPack(this FFCanvas canvas)
        {
            var list = canvas.SurfaceDescriptors;
            var count = 0;
            foreach (var descr in list)
                count += descr.SubmeshDescriptors.Count;
            if (count == 0)
                return;

            foreach (var descr in list) {
                for (var i = descr.SubmeshDescriptors.Count - 1; i >= 0; i--)
                    descr.Normalize(i); // normalize all
            }

            var width = Mathf.CeilToInt(Mathf.Sqrt(count)); // next biggest square fitting all cells
            var height = Mathf.CeilToInt(count / (float)width);
            var scale = new Vector2(1f / width, 1f / height);
            canvas.Resolution.y = Mathf.CeilToInt(canvas.Resolution.x * (height / (float)width));
            var texelSize = new Vector2(1f / (float)canvas.Resolution.x, 1f / (float)canvas.Resolution.y);

            var index = 0;
            for (var l = 0; l < list.Count; l++) {
                for (var i = list[l].SubmeshDescriptors.Count - 1; i >= 0; i--) {
                    var descr = list[l].SubmeshDescriptors[i];
                    descr.Scale(scale);
                    descr.Scale(new Vector2(1f - 2f * width * texelSize.x, 1f - 2f * height * texelSize.y));
                    descr.UVOffset.x += (index % width) / (float)width + texelSize.x;
                    descr.UVOffset.y += (index / width) / (float)height + texelSize.y;
                    list[l].SubmeshDescriptors[i] = descr;
                    index++;
                }
            }
        }

        public static bool TryGetSurfaceInfo(this List<Surface> surfaces, Transform target, int submeshIndex, out SurfaceSubmeshInfo info)
        {
            for (var i = 0; i < surfaces.Count; i++) {
                if (surfaces[i].Renderer.transform != target)
                    continue;
                for (var n = 0; n < surfaces[i].SubmeshDescriptors.Length; n++) {
                    if (!surfaces[i].SubmeshDescriptors[n].SubmeshMask.IsBitSet(submeshIndex))
                        continue;
                    info = new SurfaceSubmeshInfo(surfaces[i].Mesh, submeshIndex, surfaces[i].UVSet, surfaces[i].SubmeshDescriptors[n].AtlasTransform);
                    return true;
                }
            }
            info = default;
            return false;
        }

        public static bool TryGetSurfaceInfo(this List<Surface> surfaces, RaycastHit hitInfo, out SurfaceSubmeshInfo info)
        {
            var meshCollider = hitInfo.collider as MeshCollider;
            if (!meshCollider) {
                info = default;
                return false;
            }
            for (var i = 0; i < surfaces.Count; i++) {
                if (surfaces[i].Mesh != meshCollider.sharedMesh)
                    continue;
                var mesh = surfaces[i].Mesh;

                // find submesh index from hitInfo.triangleIndex
                var submeshIndex = -1;
                var hitVertexIndex = hitInfo.triangleIndex * 3;
                for (var s = mesh.subMeshCount - 1; s >= 0; s--) {
                    var descr = mesh.GetSubMesh(s);
                    if (descr.indexStart <= hitVertexIndex && hitVertexIndex < descr.indexStart + descr.indexCount) {
                        submeshIndex = s;
                        break;
                    }
                }
                if (submeshIndex == -1)
                    break;

                for (var n = 0; n < surfaces[i].SubmeshDescriptors.Length; n++) {
                    if (!surfaces[i].SubmeshDescriptors[n].SubmeshMask.IsBitSet(submeshIndex))
                        continue;
                    info = new SurfaceSubmeshInfo(surfaces[i].Mesh, submeshIndex, surfaces[i].UVSet, surfaces[i].SubmeshDescriptors[n].AtlasTransform);
                    return true;
                }
            }
            info = default;
            return false;
        }
    }
}

