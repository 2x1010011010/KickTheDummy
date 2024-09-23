using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class InternalShaders
    {
        private static bool copyTextureSupport = false;
        private static int renderTargetCount = 1;
        public static int SupportedRenderTargetCount => renderTargetCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnRuntimeMethodLoad()
        {
            copyTextureSupport = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            renderTargetCount = SystemInfo.supportedRenderTargetCount;
        }

        public static void CopyTexture(RenderTexture from, RenderTexture to)
        {
            if (copyTextureSupport)
                Graphics.CopyTexture(from, to);
            else
                Graphics.Blit(from, to);
        }

        public static void CopyRT(this CommandBuffer cb, RenderTargetIdentifier from, RenderTargetIdentifier to)
        {
            if (copyTextureSupport)
                cb.CopyTexture(from, to);
            else
                cb.Blit(from, to);
        }

        public static readonly ShaderPropertyIdentifier MainTexPropertyID = "_FF_MainTex";
        public static readonly ShaderPropertyIdentifier OtherTexPropertyID = "_FF_OtherTex";
        public static readonly ShaderPropertyIdentifier DataPropertyID = "_FF_Data";
        public static readonly ShaderPropertyIdentifier ColorPropertyID = "_FF_Color";
        public static readonly ShaderPropertyIdentifier TexelSizePropertyID = "_FF_TexelSize";

        public static readonly string AtlasTransformPropertyName = "_FF_AtlasTransform";
        public static readonly string SecondaryUVKeyword = "FF_UV1";
        public static readonly string RootPath = "Hidden/FluidFlow";

        public static MaterialCache.InitializeMaterialVariant SetKeyword(string name) => (Material m, bool enabled) => m.SetKeyword(name, enabled);
        public static void SetSecondaryUV(Material material, bool secondaryUV) => material.SetKeyword(SecondaryUVKeyword, secondaryUV);
    }

    public class MaterialCache : System.IDisposable
    {
        private readonly Shader shader;
        private readonly Material[] materials;
        private readonly InitializeMaterialVariant[] variantInitializers;

        public MaterialCache(string shaderName, params InitializeMaterialVariant[] initializers)
        {
            shader = Shader.Find(shaderName);
            materials = new Material[1 << initializers.Length]; // lazily create a material cache for every possible combination of initializers
            variantInitializers = initializers;
            Application.quitting += Dispose;
        }

        public void Dispose()
        {
            for (var i = 0; i < materials.Length; i++) {
                if (materials[i] != null) {
                    Object.Destroy(materials[i]);
                    materials[i] = null;
                }
            }
        }

        public Material Get(int variant = 0)
        {
            if (!materials[variant]) {
                materials[variant] = new Material(shader) {
                    hideFlags = HideFlags.HideAndDontSave
                };
                for (int i = 0; i < variantInitializers.Length; i++)
                    variantInitializers[i].Invoke(materials[variant], variant.IsBitSet(i));
            }
            return materials[variant];
        }

        public static implicit operator Material(MaterialCache wrapper) => wrapper.Get();
        public static implicit operator PerRenderTargetVariant(MaterialCache wrapper) => new PerRenderTargetVariant(wrapper, 0);

        public delegate void InitializeMaterialVariant(Material material, bool enabled);
    }

    public readonly struct PerRenderTargetVariant
    {
        private readonly MaterialCache cache;
        private readonly int variant;

        public PerRenderTargetVariant(MaterialCache cache, int variant)
        {
            this.cache = cache;
            this.variant = variant;
        }

        public Material Get(UVSet uvSet) => cache.Get(variant + Utility.SetBit(0, uvSet == UVSet.UV1));
    }
}