using UnityEngine;

namespace FluidFlow
{
    public static class BrushExtension
    {
        public static readonly ShaderPropertyIdentifier PositionPropertyID = "_FF_Position";
        public static readonly ShaderPropertyIdentifier LinePropertyID = "_FF_Line";
        public static readonly ShaderPropertyIdentifier NormalPropertyID = "_FF_Normal";
        public static readonly ShaderPropertyIdentifier RadiusPropertyID = "_FF_Radius";
        public static readonly ShaderPropertyIdentifier RadiusInvPropertyID = "_FF_RadiusInv";
        public static readonly ShaderPropertyIdentifier ThicknessInvPropertyID = "_FF_ThicknessInv";
        public static readonly ShaderPropertyIdentifier AmountPropertyID = "_FF_Amount";
        public static readonly ShaderPropertyIdentifier FadePropertyID = "_FF_Fade";
        public static readonly ShaderPropertyIdentifier FadeInvPropertyID = "_FF_FadeInv";
        public static readonly ShaderPropertyIdentifier WriteMaskPropertyID = "_FF_WriteMask";

        public static void SetFluid(Material material, bool drawFluid) => material.SetKeyword("FF_FLUID", drawFluid);
        private static readonly MaterialCache DrawSphereCache = new MaterialCache(InternalShaders.RootPath + "/Draw/Sphere", InternalShaders.SetSecondaryUV, SetFluid);
        private static readonly MaterialCache DrawDiscCache = new MaterialCache(InternalShaders.RootPath + "/Draw/Disc", InternalShaders.SetSecondaryUV, SetFluid);
        private static readonly MaterialCache DrawCapsuleCache = new MaterialCache(InternalShaders.RootPath + "/Draw/Capsule", InternalShaders.SetSecondaryUV, SetFluid);


        /* Your custom brush shaders here

        public static readonly MaterialCache DrawCustomBrush = new MaterialCache(InternalShaders.RootPath + "/Draw/CustomBrush", InternalShaders.setSecondaryUV, SetFluid);
        
        public static void DrawCustom(this FFCanvas canvas, TextureChannelIdentifier channelId, FFBrush brush, ... )
        {
            // shader variable init ...
            // e.g. Shader.SetGlobalVector("_MyPositionVar", ... );

            // call custom brush shader
            DrawBrush(canvas, channelId, brush, InternalShaders.DrawCustomBrush);
        }

        */

        /// <summary>
        /// Draws a 3D sphere brush.
        /// </summary>
        /// <param name="center">Center of the sphere in world space.</param>
        /// <param name="radius">Radius of the sphere.</param>
        public static void DrawSphere(this FFCanvas canvas, TextureChannel channel, FFBrush brush, Vector3 center, float radius, ComponentMask mask = ComponentMask.All)
        {
            Shader.SetGlobalVector(PositionPropertyID, center);
            Shader.SetGlobalFloat(RadiusInvPropertyID, 1.0f / radius);
            DrawBrush(canvas, channel, brush, DrawSphereCache, mask);
        }

        /// <summary>
        /// Draws a 3D cylinder brush.
        /// </summary>
        /// <param name="position">Center of the disc in world space.</param>
        /// <param name="normal">Direcition the disc is poining to.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <param name="thickness">Thickness/height of the cylinder in normal direction.</param>
        public static void DrawDisc(this FFCanvas canvas, TextureChannel channel, FFBrush brush, Vector3 position, Vector3 normal, float radius, float thickness, ComponentMask mask = ComponentMask.All)
        {
            Shader.SetGlobalVector(PositionPropertyID, position);
            Shader.SetGlobalVector(NormalPropertyID, normal.normalized);
            Shader.SetGlobalFloat(RadiusPropertyID, radius);
            Shader.SetGlobalFloat(ThicknessInvPropertyID, 1.0f / thickness);
            DrawBrush(canvas, channel, brush, DrawDiscCache, mask);
        }

        /// <summary>
        /// Draws a 3D capsule brush.
        /// </summary>
        /// <param name="centerA">First center of the capsule in world space.</param>
        /// <param name="centerB">Second center of the capsule in world space.</param>
        /// <param name="radius">Radius of the capsule.</param>
        public static void DrawCapsule(this FFCanvas canvas, TextureChannel channel, FFBrush brush, Vector3 centerA, Vector3 centerB, float radius, ComponentMask mask = ComponentMask.All)
        {
            Shader.SetGlobalVector(PositionPropertyID, centerA);
            var direction = centerB - centerA;
            Shader.SetGlobalVector(LinePropertyID, new Vector4(direction.x, direction.y, direction.z, 1.0f / direction.sqrMagnitude));
            Shader.SetGlobalFloat(RadiusInvPropertyID, 1.0f / radius);
            DrawBrush(canvas, channel, brush, DrawCapsuleCache, mask);
        }

        private static PerRenderTargetVariant BrushVariant(this FFBrush brush, MaterialCache material) => new PerRenderTargetVariant(material, Utility.SetBit(1, brush.BrushType == FFBrush.Type.FLUID));
        private static void DrawBrush(FFCanvas canvas, TextureChannel channel, FFBrush brush, MaterialCache material, ComponentMask mask)
        {
            var materialVariant = BrushVariant(brush, material);
            using (var paintScope = canvas.BeginPaintScope(channel)) {
                if (paintScope.IsValid) {
                    Shader.SetGlobalColor(InternalShaders.ColorPropertyID, brush.Color);
                    Shader.SetGlobalFloat(InternalShaders.DataPropertyID, brush.Data);
                    Shader.SetGlobalFloat(FadePropertyID, 1.0f - brush.Fade);
                    Shader.SetGlobalFloat(FadeInvPropertyID, brush.Fade > 0 ? (1.0f / brush.Fade) : 1);
                    Shader.SetGlobalVector(WriteMaskPropertyID, mask.ToVec4());

                    var command = Shared.CommandBuffer();
                    command.GetTemporaryRT(0, paintScope.Target.descriptor);
                    command.CopyRT(paintScope.Target, 0);
                    command.SetGlobalTexture(InternalShaders.OtherTexPropertyID, 0);
                    command.SetRenderTarget(paintScope.Target);
                    command.DrawRenderTargets(canvas.Surfaces, materialVariant);
                    command.ReleaseTemporaryRT(0);
                    Graphics.ExecuteCommandBuffer(command);
                    command.Clear();
                }
            }
        }
    }
}