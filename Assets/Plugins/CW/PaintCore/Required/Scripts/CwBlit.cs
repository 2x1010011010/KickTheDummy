using UnityEngine;
using CW.Common;

namespace PaintCore
{
	public static class CwBlit
	{
		private static Material cachedWhite;

		private static bool cachedWhiteSet;

		private static Material cachedBlit;

		private static bool cachedBlitSet;

		private static Material cachedNormal;

		private static bool cachedNormalSet;

		private static Material cachedPremultiply;

		private static bool cachedPremultiplySet;

		private static int _Buffer     = Shader.PropertyToID("_Buffer");
		private static int _BufferSize = Shader.PropertyToID("_BufferSize");
		private static int _Texture    = Shader.PropertyToID("_Texture");
		private static int _Color      = Shader.PropertyToID("_Color");

		public static void White(RenderTexture rendertexture, Mesh mesh, int submesh, CwCoord coord)
		{
			CwHelper.BeginActive(rendertexture);

			if (mesh != null)
			{
				if (cachedWhiteSet == false)
				{
					cachedWhite    = CwCommon.BuildMaterial("Hidden/PaintCore/CwWhite");
					cachedWhiteSet = true;
				}

				GL.Clear(true, true, Color.black);

				CwCommon.Draw(cachedWhite, 0, mesh, Matrix4x4.identity, submesh, coord);
			}
			else
			{
				GL.Clear(true, true, Color.white);
			}

			CwHelper.EndActive();
		}

		public static void Blit(RenderTexture rendertexture, Mesh mesh, int submesh, Texture texture, CwCoord coord)
		{
			if (cachedBlitSet == false)
			{
				cachedBlit    = CwCommon.BuildMaterial("Hidden/PaintCore/CwBlit");
				cachedBlitSet = true;
			}

			CwHelper.BeginActive(rendertexture);

			cachedBlit.SetTexture(_Buffer, texture);
			cachedBlit.SetVector(_BufferSize, new Vector2(texture.width, texture.height));

			CwCommon.Draw(cachedBlit, 0, mesh, Matrix4x4.identity, submesh, coord);

			CwHelper.EndActive();
		}

		public static void Normal(RenderTexture rendertexture, Texture texture)
		{
			if (cachedNormalSet == false)
			{
				cachedNormal    = CwCommon.BuildMaterial("Hidden/PaintCore/CwNormal");
				cachedNormalSet = true;
			}

			cachedNormal.SetTexture(_Texture, texture);

			CwCommon.Blit(rendertexture, cachedNormal, 0);
		}

		public static void Premultiply(RenderTexture rendertexture, Texture texture, Color tint)
		{
			if (cachedPremultiplySet == false)
			{
				cachedPremultiply    = CwCommon.BuildMaterial("Hidden/PaintCore/CwPremultiply");
				cachedPremultiplySet = true;
			}

			cachedPremultiply.SetTexture(_Texture, texture);
			cachedPremultiply.SetColor(_Color, CwHelper.ToLinear(tint));

			CwCommon.Blit(rendertexture, cachedPremultiply, 0);
		}
	}
}