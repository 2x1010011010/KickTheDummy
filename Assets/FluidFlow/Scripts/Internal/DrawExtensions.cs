using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class DrawExtensions
    {
        public static readonly ShaderPropertyIdentifier AtlasTransformPropertyID = InternalShaders.AtlasTransformPropertyName;
        public static readonly MaterialCache UVUnwrap = new MaterialCache(InternalShaders.RootPath + "/UVUnwrap", InternalShaders.SetSecondaryUV);
        public static readonly MaterialCache TextureInitialization = new MaterialCache(InternalShaders.RootPath + "/TextureInitialization", InternalShaders.SetSecondaryUV);
        public static Material TextureInitializationVariant(bool useSecondaryUV) => TextureInitialization.Get(Utility.SetBit(0, useSecondaryUV));

        public static void DrawRenderTargets(this CommandBuffer command, List<Surface> surfaces, PerRenderTargetVariant materialVariant, bool onlyActive = true)
        {
            for (var i = surfaces.Count - 1; i >= 0; i--) {
                if (onlyActive && (!surfaces[i].Renderer.enabled || !surfaces[i].Renderer.gameObject.activeInHierarchy))
                    continue;
                var material = materialVariant.Get(surfaces[i].UVSet);

                for (var s = surfaces[i].SubmeshDescriptors.Length - 1; s >= 0; s--) {
                    command.SetGlobalVector(AtlasTransformPropertyID, surfaces[i].SubmeshDescriptors[s].AtlasTransform);

                    var mask = surfaces[i].SubmeshDescriptors[s].SubmeshMask;
                    for (var it = mask.IterateFlags(); it.Valid(); it.Next())
                        command.DrawRenderer(surfaces[i].Renderer, material, it.Index(), 0);
                }
            }
        }

        public static void DrawMeshes(this List<Surface> surfaces, PerRenderTargetVariant materialVariant)
        {
            for (var i = surfaces.Count - 1; i >= 0; i--)
                surfaces[i].DrawMesh(materialVariant);
        }

        public static void DrawMesh(this Surface surface, PerRenderTargetVariant materialVariant)
        {
            var material = materialVariant.Get(surface.UVSet);
            for (var s = surface.SubmeshDescriptors.Length - 1; s >= 0; s--) {
                Shader.SetGlobalVector(AtlasTransformPropertyID, surface.SubmeshDescriptors[s].AtlasTransform);
                material.SetPass(0);
                var mask = surface.SubmeshDescriptors[s].SubmeshMask;
                for (var it = mask.IterateFlags(); it.Valid(); it.Next())
                    Graphics.DrawMeshNow(surface.Mesh, Vector3.zero, Quaternion.identity, it.Index());
            }
        }

        public static void DrawUVMap(this List<Surface> surfaces, RenderTexture target)
        {
            Graphics.SetRenderTarget(target);
            GL.Clear(false, true, Color.clear);
            surfaces.DrawMeshes(UVUnwrap);
        }

        public static void InitializeTextureChannel(this List<Surface> surfaces, RenderTexture targetTex, TextureChannelDescriptor channelDescriptor)
        {
            Graphics.SetRenderTarget(targetTex);
            GL.Clear(false, true, Color.clear);
            var sharedMaterialsCache = Shared.MaterialList();
            for (var i = surfaces.Count - 1; i >= 0; i--) {
                var material = TextureInitializationVariant(surfaces[i].UVSet == UVSet.UV1);
                for (var s = surfaces[i].SubmeshDescriptors.Length - 1; s >= 0; s--) {
                    Shader.SetGlobalVector(AtlasTransformPropertyID, surfaces[i].SubmeshDescriptors[s].AtlasTransform);
                    for (var it = surfaces[i].SubmeshDescriptors[s].SubmeshMask.IterateFlags(); it.Valid(); it.Next()) {
                        if (channelDescriptor.Initialization == TextureChannelDescriptor.InitializationMode.COPY) {
                            surfaces[i].Renderer.GetSharedMaterials(sharedMaterialsCache);
                            if (channelDescriptor.TextureChannelReference.IsValid && channelDescriptor.TextureChannelReference.Resolve().TryGet(sharedMaterialsCache[it.Index()], out var texture))
                                Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, texture);
                            else
                                Debug.LogWarningFormat("FluidFlow: Copy initialization failed. '{0}' of '{1}' has no property '{2}'.", sharedMaterialsCache[it.Index()], surfaces[i].Renderer, channelDescriptor.TextureChannelReference.Identifier);
                        } else {
                            channelDescriptor.Initialization.SetGlobalShaderColor();
                        }
                        material.SetPass(0);
                        Graphics.DrawMeshNow(surfaces[i].Mesh, Vector3.zero, Quaternion.identity, it.Index());
                    }
                }
            }
        }

        private static void SetGlobalShaderColor(this TextureChannelDescriptor.InitializationMode mode)
        {
            Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, mode switch {
                TextureChannelDescriptor.InitializationMode.WHITE => Texture2D.whiteTexture,
                TextureChannelDescriptor.InitializationMode.BLACK => Texture2D.blackTexture,
                TextureChannelDescriptor.InitializationMode.BUMP => Texture2D.normalTexture,
                TextureChannelDescriptor.InitializationMode.RED => Texture2D.redTexture,
                _ => Texture2D.grayTexture,
            });
        }
    }
}