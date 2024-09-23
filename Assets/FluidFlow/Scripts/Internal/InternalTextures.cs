
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class InternalTextures
    {
        // rgba formats with can represent values between 0-1 with over 8bit precision
        private static readonly GraphicsFormat[] RGBA_HIGHPREC_0_1_FORMATS = new GraphicsFormat[] {
            GraphicsFormat.R16G16B16A16_UNorm,
            GraphicsFormat.R16G16B16A16_SNorm,
            GraphicsFormat.R16G16B16A16_SFloat,
            GraphicsFormat.R32G32B32A32_SFloat
        };
        private static GraphicsFormat rgba_highprec_0_1_format = GraphicsFormat.None;
        public static GraphicsFormat HighPrecisionRGBA {
            get {
                if (rgba_highprec_0_1_format == GraphicsFormat.None)
                    rgba_highprec_0_1_format = RGBA_HIGHPREC_0_1_FORMATS.GetSupportedFormat();
                return rgba_highprec_0_1_format;
            }
        }

        // basic rgba color format with minimum 8 bit per channel
        private static readonly GraphicsFormat[] RGBA_COLOR_FORMATS = new GraphicsFormat[] {
            GraphicsFormat.R8G8B8A8_UNorm,
            GraphicsFormat.R16G16B16A16_UNorm,
            GraphicsFormat.R16G16B16A16_SFloat,
            GraphicsFormat.R32G32B32A32_SFloat,
        };
        private static GraphicsFormat rgba_color_format = GraphicsFormat.None;
        public static GraphicsFormat ColorFormatRGBA {
            get {
                if (rgba_color_format == GraphicsFormat.None)
                    rgba_color_format = RGBA_COLOR_FORMATS.GetSupportedFormat();
                return rgba_color_format;
            }
        }

        // smallest possible single red channel rendertexture
        private static readonly GraphicsFormat[] R8_MIN_FORMATS = new GraphicsFormat[] {
            GraphicsFormat.R8_UNorm,
            GraphicsFormat.R16_UNorm,
            GraphicsFormat.R16_SFloat,
            GraphicsFormat.R32_SFloat,
        };
        private static GraphicsFormat r8_min_format = GraphicsFormat.None;
        public static GraphicsFormat R8MinFormat {
            get {
                if (r8_min_format == GraphicsFormat.None)
                    r8_min_format = R8_MIN_FORMATS.GetSupportedFormat();
                return r8_min_format;
            }
        }

        public static RenderTexture CreateRenderTexture(GraphicsFormat format, Vector2Int resolution)
        {
            var rt = new RenderTexture(resolution.x, resolution.y, 0, format, 0) {
                autoGenerateMips = false,
                useMipMap = false
            };
            rt.Create();
            return rt;
        }
        public static RenderTexture CreateRenderTexture(TextureChannelFormat format, Vector2Int resolution) => CreateRenderTexture(format.Format, resolution);
    }

    /// <summary>
    /// RAII for getting/ releasing temporary rendertextures
    /// </summary>
    public struct TmpRenderTexture : IDisposable
    {
        private readonly RenderTexture texture;

        public TmpRenderTexture(RenderTextureDescriptor descr)
        {
            texture = create(descr);
        }

        public TmpRenderTexture(GraphicsFormat format, int width, int height)
        {
            texture = create(new RenderTextureDescriptor(width, height, format, 0, 0));
        }

        private static RenderTexture create(RenderTextureDescriptor descr)
        {
            var texture = RenderTexture.GetTemporary(descr);
            texture.filterMode = FilterMode.Point;
            return texture;
        }

        public static implicit operator RenderTexture(TmpRenderTexture tmpRT)
        {
            return tmpRT.texture;
        }

        public void Dispose()
        {
            RenderTexture.ReleaseTemporary(texture);
        }
    }
}