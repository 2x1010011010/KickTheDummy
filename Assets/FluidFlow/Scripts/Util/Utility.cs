using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class Utility
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static void DebugTexture(this Texture tex)
        {
            var id = "Debug_" + tex.name + "_" + tex.GetInstanceID();
            var debugGO = GameObject.Find(id);
            if (!debugGO) {
                debugGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
                debugGO.name = id;
            }
            debugGO.GetComponent<Renderer>().material.mainTexture = tex;
        }

        public static void DebugFrustum(Matrix4x4 frustum)
        {
            Matrix4x4 inv = frustum.inverse;
            var aaa = inv.MultiplyPoint(new Vector3(1, 1, 1));
            var aab = inv.MultiplyPoint(new Vector3(1, 1, -1));
            var aba = inv.MultiplyPoint(new Vector3(1, -1, 1));
            var abb = inv.MultiplyPoint(new Vector3(1, -1, -1));
            var baa = inv.MultiplyPoint(new Vector3(-1, 1, 1));
            var bab = inv.MultiplyPoint(new Vector3(-1, 1, -1));
            var bba = inv.MultiplyPoint(new Vector3(-1, -1, 1));
            var bbb = inv.MultiplyPoint(new Vector3(-1, -1, -1));

            UnityEngine.Debug.DrawLine(aaa, aba);
            UnityEngine.Debug.DrawLine(aba, bba);
            UnityEngine.Debug.DrawLine(bba, baa);
            UnityEngine.Debug.DrawLine(baa, aaa);
            UnityEngine.Debug.DrawLine(aab, abb);
            UnityEngine.Debug.DrawLine(abb, bbb);
            UnityEngine.Debug.DrawLine(bbb, bab);
            UnityEngine.Debug.DrawLine(bab, aab);
            UnityEngine.Debug.DrawLine(aaa, aab);
            UnityEngine.Debug.DrawLine(aba, abb);
            UnityEngine.Debug.DrawLine(baa, bab);
            UnityEngine.Debug.DrawLine(bba, bbb);
        }

        public static Matrix4x4 OrthogonalProjector(Transform transform, float width, float height, float near, float far)
        {
            return Matrix4x4.Ortho(-width * .5f, width * .5f, -height * .5f, height * .5f, near, far) * transform.worldToLocalMatrix;
        }

        public static Matrix4x4 PerspectiveProjector(Transform transform, float fov, float aspect, float near, float far)
        {
            return Matrix4x4.Perspective(fov, aspect, near, far) * transform.worldToLocalMatrix;
        }

        public static Mesh GetMesh(this Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer)
                return (renderer as SkinnedMeshRenderer).sharedMesh;
            else if (renderer is MeshRenderer) {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter != null)
                    return filter.sharedMesh;
            }
            return null;
        }

        public static void SetMesh(this Renderer renderer, Mesh mesh)
        {
            if (renderer is SkinnedMeshRenderer)
                (renderer as SkinnedMeshRenderer).sharedMesh = mesh;
            else if (renderer is MeshRenderer) {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter)
                    filter.sharedMesh = mesh;
            }
        }

        public static int[] GetSubmeshIndices(this Mesh mesh, int submeshMask)
        {
            var submeshList = new List<ValueTuple<int, SubMeshDescriptor>>();
            for (var it = submeshMask.IterateFlags(); it.Valid(); it.Next())
                submeshList.Add(new ValueTuple<int, SubMeshDescriptor>(it.Index(), mesh.GetSubMesh(it.Index())));
            var indices = new int[submeshList.Sum(submesh => submesh.Item2.indexCount)];
            int index = 0;
            foreach (var submesh in submeshList) {
                Array.Copy(mesh.GetTriangles(submesh.Item1), 0, indices, index, submesh.Item2.indexCount);
                index += submesh.Item2.indexCount;
            }
            return indices;
        }

        public static bool HasUV1AndTransformations(this Mesh mesh)
        {
            return mesh.HasVertexAttribute(VertexAttribute.TexCoord1) && mesh.HasVertexAttribute(VertexAttribute.TexCoord2);
        }

        public static Mesh CopyMesh(Mesh source)
        {
            var copy = UnityEngine.Object.Instantiate(source);
            copy.name = source.name + "_FF_Copy";
            return copy;
        }

        public static void OverwriteExistingMesh(Mesh data, Mesh target)
        {
            Mesh.ApplyAndDisposeWritableMeshData(Mesh.AcquireReadOnlyMeshData(data), target);
        }

        public static int GetTriangleCount(this Mesh.MeshData meshData)
        {
            int triangleCount = 0;
            for (int i = 0; i < meshData.subMeshCount; i++)
                triangleCount += meshData.GetSubMesh(i).indexCount;
            return triangleCount;
        }

        public static void GetTriangles(this Mesh.MeshData meshData, Unity.Collections.NativeArray<int> triangles)
        {
            for (int i = 0; i < meshData.subMeshCount; i++) {
                var descr = meshData.GetSubMesh(i);
                meshData.GetIndices(triangles.GetSubArray(descr.indexStart, descr.indexCount), i);
            }
        }

        public static Vector2[] GetUVSet(this Mesh mesh, UVSet uvSet)
        {
            return uvSet == UVSet.UV1 ? mesh.uv2 : mesh.uv;
        }

        public static int ValidateSubmeshMask(this Mesh mesh, int submeshMask)
        {
            return (int)((uint)submeshMask & ((1u << mesh.subMeshCount) - 1));
        }

        public struct BitMaskIterator
        {
            private readonly int Mask;
            private int submeshIndex;

            public BitMaskIterator(int mask)
            {
                submeshIndex = 0;
                Mask = mask;
                SkipZeros();
            }

            public bool Valid()
            {
                return (Mask & (1u << submeshIndex)) != 0u;
            }

            public int Index()
            {
                return submeshIndex;
            }

            private void SkipZeros()
            {
                for (; ((1u << submeshIndex) & Mask) == 0u && (1u << submeshIndex) <= Mask; submeshIndex++)
                    if (Mask.IsBitSet(submeshIndex))
                        return;
            }

            public void Next()
            {
                submeshIndex++;
                SkipZeros();
            }
        }

        public static BitMaskIterator IterateFlags(this int flags)
        {
            return new BitMaskIterator(flags);
        }

        public static IEnumerable<int> EnumerateSetBits(this int flags)
        {
            for (var it = flags.IterateFlags(); it.Valid(); it.Next())
                yield return it.Index();
        }

        public static bool IsBitSet(this int flags, int index)
        {
            return (((uint)flags) & (1u << index)) != 0u;
        }

        public static int SetBit(int index, bool enabled)
        {
            return (int)(enabled ? 1u << index : 0u);
        }

        public static void SetKeyword(this Material material, string keyword, bool enabled)
        {
            if (enabled)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        public static Vector4 GetTexelSize(this RenderTexture texture)
        {
            return new Vector4(1.0f / texture.width, 1.0f / texture.height, texture.width, texture.height);
        }

        public static float ManhattanDistance(this Vector4 vec)
        {
            return Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z) + Mathf.Abs(vec.w);
        }

        public static ScopedTextureFiltering SetTemporaryFilterMode(this Texture tex, FilterMode filterMode)
        {
            return new ScopedTextureFiltering(tex, filterMode);
        }

        public static Texture2D GetDefaultTexture(this Material material, string texturePropertyName)
        {
            var shader = material.shader;
            var index = shader.FindPropertyIndex(texturePropertyName);
            if (index >= 0) {
                // according to unity's documentation, this are the only valid default textures, all other strings default to gray
                // https://docs.unity3d.com/Manual/SL-Properties.html
                switch (shader.GetPropertyTextureDefaultName(index)) {
                    case "white":
                        return Texture2D.whiteTexture;

                    case "black":
                        return Texture2D.blackTexture;

                    case "bump":
                        return Texture2D.normalTexture;

                    case "red":
                        return Texture2D.redTexture;

                    case "gray":
                    default:
                        return Texture2D.grayTexture;
                }
            }
            return Texture2D.grayTexture;
        }

        public static ReadbackRenderTextureRequest RequestReadback(this RenderTexture rt, TextureFormat destinationFormat = TextureFormat.RGBA32, bool forceNonAsync = false)
        {
            return new ReadbackRenderTextureRequest(rt, destinationFormat, forceNonAsync);
        }

        public static void SaveAsPNG(this Texture2D texture, string path)
        {
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        }
    }

    public class ReadbackRenderTextureRequest : IEnumerator
    {
        private bool requestRunning = true;
        private bool success = false;
        private RenderTexture source;
        private Texture2D result;

        public Texture2D Result(bool apply = true)
        {
            if (!success) {
                // no valid result yet? -> read texture blocking
                using (RestoreRenderTarget.RestoreActive()) {
                    Graphics.SetRenderTarget(source);
                    result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                    success = true;
                    requestRunning = false;
                }
            }
            if (apply)
                result.Apply();
            return result;
        }

        public ReadbackRenderTextureRequest(RenderTexture source, TextureFormat format, bool forceNonAsync = false)
        {
            this.source = source;
            result = new Texture2D(source.width, source.height, format, false);
            result.filterMode = source.filterMode;
            if (!forceNonAsync && SystemInfo.supportsAsyncGPUReadback) {
                AsyncGPUReadback.Request(source, 0, format, (request) => {
                    success = !request.hasError;
                    if (success)
                        result.LoadRawTextureData(request.GetData<byte>());
                    requestRunning = false;
                });
            } else {
                requestRunning = false;
            }
        }

        public object Current { get { return null; } }

        public bool MoveNext()
        {
            return requestRunning;
        }

        public void Reset()
        {
        }
    }

    public struct ScopedMaterialPropertyBlockEdit : IDisposable
    {
        public readonly MaterialPropertyBlock PropertyBlock;
        private readonly Renderer renderer;
        private readonly int index;

        public ScopedMaterialPropertyBlockEdit(Renderer renderer, int index)
        {
            this.renderer = renderer;
            this.index = index;
            PropertyBlock = Shared.MaterialPropertyBlock();
            renderer.GetPropertyBlock(PropertyBlock, index);
        }

        public void Dispose()
        {
            renderer.SetPropertyBlock(PropertyBlock, index);
        }
    }

    public readonly struct RestoreRenderTarget : IDisposable
    {
        private readonly RenderTexture tmp;

        public RestoreRenderTarget(RenderTexture rt)
        {
            tmp = rt;
        }

        public void Dispose()
        {
            RenderTexture.active = tmp;
        }

        public static RestoreRenderTarget RestoreActive()
        {
            return new RestoreRenderTarget(RenderTexture.active);
        }
    }

    public readonly struct ScopedTextureFiltering : IDisposable
    {
        private readonly Texture texture;
        private readonly FilterMode tmpFM;

        public ScopedTextureFiltering(Texture tex, FilterMode filterMode)
        {
            texture = tex;
            tmpFM = tex.filterMode;
            tex.filterMode = filterMode;
        }

        public void Dispose()
        {
            texture.filterMode = tmpFM;
        }
    }

    public readonly struct ShaderPropertyIdentifier
    {
        public readonly int ShaderPropertyId;

        public ShaderPropertyIdentifier(string name)
        {
            ShaderPropertyId = Shader.PropertyToID(name);
        }

        public ShaderPropertyIdentifier(int id)
        {
            ShaderPropertyId = id;
        }

        public static implicit operator ShaderPropertyIdentifier(string name)
        {
            return new ShaderPropertyIdentifier(name);
        }

        public static implicit operator int(ShaderPropertyIdentifier identifier)
        {
            return identifier.ShaderPropertyId;
        }
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        struct SerializableKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public SerializableKeyValuePair(KeyValuePair<TKey, TValue> kvPair)
            {
                Key = kvPair.Key;
                Value = kvPair.Value;
            }
        }
        [SerializeField] private List<SerializableKeyValuePair> data = new List<SerializableKeyValuePair>();

        public void OnBeforeSerialize()
        {
            data.Clear();
            foreach (var pair in this)
                data.Add(new SerializableKeyValuePair(pair));
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < data.Count; i++)
                Add(data[i].Key, data[i].Value);
            data.Clear();
        }
    }


    [Serializable]
    public struct Optional<T>
    {
        [SerializeField] private bool enabled;
        [SerializeField] private T value;

        public bool Enabled => enabled;
        public T Value => value;

        public Optional(T initialValue)
        {
            enabled = true;
            value = initialValue;
        }

        public bool TryGet(out T val)
        {
            val = value;
            return enabled;
        }

        public static Optional<T> None => new Optional<T>();
    }

    [Serializable]
    public struct Tracked<T> : ISerializationCallbackReceiver where T : UnityEngine.Object
    {
        [SerializeField] private T inspectorValue;  // used for tracking changes made in the inspector
        private T target;
        public T Target {
            get => target;
            set {
                if (target != value) {
                    // Debug.Log("OnChange");
                    OnBeforeChanged?.Invoke(target);
                    target = value;
                    OnAfterChanged?.Invoke(target);
                }
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (OnBeforeChanged != null && OnAfterChanged != null)  // only set during runtime
                Target = inspectorValue;
        }

        public void Initialize()
        {
            Reset();
            Target = inspectorValue;
        }

        public void Reset()
        {
            Target = null;
        }

        public event Action<T> OnBeforeChanged;
        public event Action<T> OnAfterChanged;
    }
}