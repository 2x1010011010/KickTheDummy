using System.Collections;
using UnityEngine;

namespace FluidFlow
{
    public readonly struct PaintScope : System.IDisposable
    {
        public readonly RenderTexture Target;
        public readonly bool IsValid;
        private readonly FFCanvas canvas;
        private readonly TextureChannel channel;
        private readonly bool notify;

        public PaintScope(FFCanvas canvas, TextureChannel channel, bool notify)
        {
            this.canvas = canvas;
            this.channel = channel;
            this.notify = notify;
            IsValid = canvas.TextureChannels.TryGetValue(channel, out Target);
        }

        public void Dispose()
        {
            if (IsValid && notify)
                canvas.OnTextureChannelUpdated.Invoke(channel);
        }
    }

    public static class FFCanvasExtensions
    {
        /// <summary>
        /// Convenience function for creating a disposable PaintScope, for painting on a TextureChannel of a FFCanvas.
        /// </summary>
        public static PaintScope BeginPaintScope(this FFCanvas canvas, TextureChannel channel, bool notify = true)
        {
            return new PaintScope(canvas, channel, notify);
        }

        /// <summary>
        /// Readback and save texture channel of a FFCanvas as a png
        /// </summary>
        public static void SaveTextureChannel(this FFCanvas canvas, TextureChannel identifier, string path, TextureFormat destinationFormat = TextureFormat.ARGB32)
        {
            IEnumerator save()
            {
                var request = canvas.TextureChannels[identifier].RequestReadback(destinationFormat);
                yield return request;
                request.Result(false).SaveAsPNG(path);
            };
            canvas.StartCoroutine(save());
        }

        private static readonly MaterialCache QueryPaint =
                new MaterialCache(InternalShaders.RootPath + "/Extensions/QueryPaint",
                    InternalShaders.SetSecondaryUV);

        private static readonly MaterialCache QueryPaintConvolution =
                new MaterialCache(InternalShaders.RootPath + "/Extensions/QueryPaintConvolution");

        public static float QueryPaintApplied(this FFCanvas canvas, TextureChannel identifier, Color matchMin, Color matchMax)
        {
            if (canvas.TextureChannels.TryGetValue(identifier, out RenderTexture target)) {
                int texSize = target.width;
                var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
                var tmpA = RenderTexture.GetTemporary(texSize, texSize, 0, format);
                if (!tmpA.IsCreated())
                    tmpA.Create();
                Graphics.SetRenderTarget(tmpA);
                GL.Clear(false, true, new Color(-1, -1, -1, -1));
                Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, target);
                Shader.SetGlobalColor("_FF_Min", matchMin);
                Shader.SetGlobalColor("_FF_Max", matchMax);
                canvas.Surfaces.DrawMeshes(QueryPaint);
                // convolute to make readback faster
                for (texSize /= 2; texSize >= 1; texSize /= 2) {
                    var tmpB = RenderTexture.GetTemporary(texSize, texSize, 0, format);
                    if (!tmpB.IsCreated())
                        tmpB.Create();
                    Graphics.Blit(tmpA, tmpB, QueryPaintConvolution);
                    RenderTexture.ReleaseTemporary(tmpA);
                    tmpA = tmpB;
                }
                var readback = tmpA.RequestReadback(TextureFormat.RFloat, true);
                RenderTexture.ReleaseTemporary(tmpA);
                return readback.Result(false).GetPixel(0, 0).r;
            } else {
                return 0;
            }
        }

        public static bool AtlasTransformUV(this FFCanvas canvas, Transform target, int submesh, Vector2 inUV, out Vector2 canvasUV)
        {
            if (canvas.Surfaces.TryGetSurfaceInfo(target, submesh, out var info)) {
                var transform = info.AtlasTransform;
                canvasUV = new Vector2(inUV.x * transform.x + transform.z, inUV.y * transform.y + transform.w);
                return true;
            } else {
                canvasUV = inUV;
                return false;
            }
        }

        public static bool TryGetCanvasUV(this FFCanvas canvas, in RaycastHit hitInfo, out Vector2 canvasUV)
        {
            if (canvas.Surfaces.TryGetSurfaceInfo(hitInfo, out var info)) {
                var uv = info.UVSet == UVSet.UV0 ? hitInfo.textureCoord : hitInfo.textureCoord2;
                var transform = info.AtlasTransform;
                canvasUV = new Vector2(uv.x * transform.x + transform.z, uv.y * transform.y + transform.w);
                return true;
            } else {
                canvasUV = hitInfo.textureCoord;
                return false;
            }
        }
    }
}