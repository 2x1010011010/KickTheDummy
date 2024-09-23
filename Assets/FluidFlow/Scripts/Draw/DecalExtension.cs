using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class DecalExtension
    {
        public static readonly ShaderPropertyIdentifier DecalTexPropertyID = "_FF_DecalTex";
        public static readonly ShaderPropertyIdentifier MaskTexPropertyID = "_FF_MaskTex";
        public static readonly ShaderPropertyIdentifier MaskComponentsPropertyID = "_FF_MaskComponents";
        public static readonly ShaderPropertyIdentifier MaskComponentsInvPropertyID = "_FF_MaskComponentsInv";
        public static readonly ShaderPropertyIdentifier FadeOnAngledSurfacePropertyID = "_FF_FadeOnAngledSurface";
        public static readonly ShaderPropertyIdentifier PaintBackfacingPropertyID = "_FF_PaintBackfacingSurface";
        public static readonly ShaderPropertyIdentifier WriteMaskPropertyID = "_FF_WriteMask";
        public static void SetMask(Material material, bool mask) => material.SetKeyword("FF_MASK", mask);
        public static void SetFluid(Material material, bool fluid) => material.SetKeyword("FF_FLUID", fluid);
        public static void SetNormal(Material material, bool normal) => material.SetKeyword("FF_NORMAL", normal);
        public static void SetColor(Material material, bool color) => material.SetKeyword("FF_COLOR", color);

        public static readonly MaterialCache Projection = new MaterialCache(InternalShaders.RootPath + "/Draw/Projection", InternalShaders.SetSecondaryUV, SetMask, SetFluid, SetNormal, SetColor);
        public static readonly MaterialCache UVDecal = new MaterialCache(InternalShaders.RootPath + "/Draw/UVDecal", InternalShaders.SetSecondaryUV, InternalShaders.SetKeyword("FF_SOURCE_UV1"), SetMask, SetFluid, SetNormal, SetColor);

        public static PerRenderTargetVariant ProjectionVariant(FFDecal.Channel channel, bool useMask)
            => new PerRenderTargetVariant(Projection,
                            Utility.SetBit(1, useMask)
                          | Utility.SetBit(2, channel.ChannelType == FFDecal.Channel.Type.FLUID)
                          | Utility.SetBit(3, channel.ChannelType == FFDecal.Channel.Type.NORMAL)
                          | Utility.SetBit(4, channel.Source.SourceType == FFDecal.ColorSource.Type.COLOR));

        public static PerRenderTargetVariant UVDecalVariant(FFDecal.Channel channel, bool useMask, UVSet sourceUVSet)
            => new PerRenderTargetVariant(UVDecal,
                            Utility.SetBit(1, sourceUVSet == UVSet.UV1)
                          | Utility.SetBit(2, useMask)
                          | Utility.SetBit(3, channel.ChannelType == FFDecal.Channel.Type.FLUID)
                          | Utility.SetBit(4, channel.ChannelType == FFDecal.Channel.Type.NORMAL)
                          | Utility.SetBit(5, channel.Source.SourceType == FFDecal.ColorSource.Type.COLOR));

        /// <summary>
        /// Project decal onto canvas in world space.
        /// </summary>
        /// <param name="projector">Wrapper around a world space view-projection matrix.</param>
        /// <param name="paintBackfacing">Only paint surfaces pointing towards the projector.</param>
        public static void ProjectDecal(this FFCanvas canvas, FFDecal decal, FFProjector projector, bool fadeBasedOnSurfaceAngle = true, bool paintBackfacing = false)
        {
            Shader.SetGlobalFloat(PaintBackfacingPropertyID, paintBackfacing ? 1 : 0);
            Shader.SetGlobalFloat(FadeOnAngledSurfacePropertyID, fadeBasedOnSurfaceAngle ? 1 : 0);
            SetupDecalMask(decal.MaskChannel);
            var hasMask = decal.MaskChannel.Texture != null;

            var command = Shared.CommandBuffer();
            command.SetViewProjectionMatrices(projector.View, projector.Projection);
            foreach (var channel in decal.Channels) {
                using (var paintScope = canvas.BeginPaintScope(channel.TargetTextureChannel)) {
                    if (!paintScope.IsValid)
                        continue;
                    SetupDecalChannel(command, channel);
                    command.GetTemporaryRT(0, paintScope.Target.descriptor);
                    command.CopyRT(paintScope.Target, 0);
                    command.SetGlobalTexture(InternalShaders.OtherTexPropertyID, 0);
                    command.SetRenderTarget(paintScope.Target);
                    var materialVariant = ProjectionVariant(channel, hasMask);
                    command.DrawRenderTargets(canvas.Surfaces, materialVariant);
                    command.ReleaseTemporaryRT(0);
                }
            }
            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }

        /// <summary>
        /// Draw decal onto canvas in uv space.
        /// </summary>
        public static void DrawDecal(this FFCanvas canvas, FFDecal decal, int surfacesMask = -1, UVSet decalTextureUVSet = UVSet.UV0)
        {
            var validatedMask = surfacesMask & ((1 << canvas.Surfaces.Count) - 1);
            SetupDecalMask(decal.MaskChannel);

            foreach (var channel in decal.Channels) {
                using (var paintScope = canvas.BeginPaintScope(channel.TargetTextureChannel)) {
                    if (!paintScope.IsValid)
                        continue;
                    SetupDecalChannel(channel);
                    using (var tmp = new TmpRenderTexture(paintScope.Target.descriptor)) {
                        InternalShaders.CopyTexture(paintScope.Target, tmp);
                        Shader.SetGlobalTexture(InternalShaders.OtherTexPropertyID, tmp);
                        Graphics.SetRenderTarget(paintScope.Target);
                        for (var it = validatedMask.IterateFlags(); it.Valid(); it.Next())
                            canvas.Surfaces[it.Index()].DrawMesh(UVDecalVariant(channel, decal.MaskChannel.Texture != null, decalTextureUVSet));
                    }
                }
            }
        }

        public static void SetupDecalChannel(FFDecal.Channel channel)
        {
            switch (channel.ChannelType) {
                case FFDecal.Channel.Type.NORMAL:
                case FFDecal.Channel.Type.FLUID:
                    Shader.SetGlobalFloat(InternalShaders.DataPropertyID, channel.Data);
                    break;
            }
            switch (channel.Source.SourceType) {
                case FFDecal.ColorSource.Type.TEXTURE:
                    Shader.SetGlobalTexture(DecalTexPropertyID, channel.Source.Texture);
                    break;

                case FFDecal.ColorSource.Type.COLOR:
                    Shader.SetGlobalColor(InternalShaders.ColorPropertyID, channel.Source.Color);
                    break;
            }
        }

        public static void SetupDecalChannel(CommandBuffer cb, FFDecal.Channel channel)
        {
            switch (channel.ChannelType) {
                case FFDecal.Channel.Type.NORMAL:
                case FFDecal.Channel.Type.FLUID:
                    cb.SetGlobalFloat(InternalShaders.DataPropertyID, channel.Data);
                    break;
            }
            switch (channel.Source.SourceType) {
                case FFDecal.ColorSource.Type.TEXTURE:
                    cb.SetGlobalTexture(DecalTexPropertyID, channel.Source.Texture);
                    break;

                case FFDecal.ColorSource.Type.COLOR:
                    cb.SetGlobalColor(InternalShaders.ColorPropertyID, channel.Source.Color);
                    break;
            }
            cb.SetGlobalVector(WriteMaskPropertyID, channel.WriteMask.ToVec4());
        }

        public static void SetupDecalMask(FFDecal.Mask mask)
        {
            if (mask.Texture) {
                Shader.SetGlobalTexture(MaskTexPropertyID, mask.Texture);
                var componentsMask = mask.Components.ToVec4();
                var manhattan = componentsMask.ManhattanDistance();
                Shader.SetGlobalVector(MaskComponentsPropertyID, componentsMask);
                Shader.SetGlobalFloat(MaskComponentsInvPropertyID, manhattan == 0 ? 1 : (1f / manhattan));
            }
        }
    }

    public struct FFProjector
    {
        private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Matrix4x4 ViewProjection => Projection * View;

        public FFProjector(Matrix4x4 view, Matrix4x4 projection)
        {
            View = view;
            Projection = projection;
        }

        // allow implicitly converting a projector to a view-projection matrix.
        public static implicit operator Matrix4x4(FFProjector projector)
        {
            return projector.ViewProjection;
        }

        /// <summary>
        /// Convenience function for creating a orthogonal view-projection from a transform, pointing in local z direction.
        /// </summary>
        public static FFProjector Orthogonal(Transform transform, float width, float height, float near, float far)
        {
            return new FFProjector(transform.worldToLocalMatrix,
                                   Matrix4x4.Ortho(-width * .5f, width * .5f, -height * .5f, height * .5f, near, far) * flipZ);
        }

        public static FFProjector Orthogonal(Ray ray, Vector3 up, float width, float height, float near, float far)
        {
            return new FFProjector(Matrix4x4.TRS(ray.origin, Quaternion.LookRotation(ray.direction, up), Vector3.one).inverse,
                                   Matrix4x4.Ortho(-width * .5f, width * .5f, -height * .5f, height * .5f, near, far) * flipZ);
        }

        /// <summary>
        /// Convenience function for creating a perspective view-projection from a transform, pointing in local z direction.
        /// </summary>
        public static FFProjector Perspective(Transform transform, float fov, float aspect, float near, float far)
        {
            return new FFProjector(transform.worldToLocalMatrix, Matrix4x4.Perspective(fov, aspect, near, far) * flipZ);
        }

        public static FFProjector Perspective(Ray ray, Vector3 up, float fov, float aspect, float near, float far)
        {
            return new FFProjector(Matrix4x4.TRS(ray.origin, Quaternion.LookRotation(ray.direction, up), Vector3.one).inverse,
                                   Matrix4x4.Perspective(fov, aspect, near, far) * flipZ);
        }
    }
}