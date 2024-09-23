using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This class manages the fill painting command.</summary>
	public class CwCommandFill : CwCommand
	{
		public CwBlendMode     Blend;
		public CwHashedTexture Texture;
		public Color           Color;
		public float           Opacity;
		public float           Minimum;

		public static CwCommandFill Instance = new CwCommandFill();

		private static Stack<CwCommandFill> pool = new Stack<CwCommandFill>();

		private static Material cachedMaterial;

		private static int cachedMaterialHash;

		public override bool RequireMesh { get { return false; } }

		private static int _Buffer     = Shader.PropertyToID("_Buffer");
		private static int _BufferSize = Shader.PropertyToID("_BufferSize");
		private static int _Texture    = Shader.PropertyToID("_Texture");
		private static int _Color      = Shader.PropertyToID("_Color");
		private static int _Opacity    = Shader.PropertyToID("_Opacity");
		private static int _Minimum    = Shader.PropertyToID("_Minimum");

		static CwCommandFill()
		{
			BuildMaterial(ref cachedMaterial, ref cachedMaterialHash, "Hidden/PaintCore/CwFill");
		}

		public static RenderTexture Blit(RenderTexture main, CwBlendMode blendMode, Texture texture, Color color, float opacity, float minimum)
		{
			var swap = CwCommon.GetRenderTexture(main.descriptor, main);

			Blit(ref main, ref swap, blendMode, texture, color, opacity, minimum);

			CwCommon.ReleaseRenderTexture(swap);

			return main;
		}

		public static void Blit(ref RenderTexture main, ref RenderTexture swap, CwBlendMode blendMode, Texture texture, Color color, float opacity, float minimum)
		{
			var material = Instance.SetMaterial(blendMode, texture, color, opacity, minimum);

			Instance.Apply(material);

			//if (doubleBuffer == true)
			{
				CwCommandReplace.Blit(swap, main, Color.white);

				material.SetTexture(_Buffer, swap);
				material.SetVector(_BufferSize, new Vector2(swap.width, swap.height));
			}

			CwCommon.Blit(main, material, blendMode);
		}

		public override void Apply(Material material)
		{
			base.Apply(material);

			Blend.Apply(material);

			material.SetTexture(_Texture, Texture);
			material.SetColor(_Color, CwHelper.ToLinear(Color));
			material.SetFloat(_Opacity, Opacity);
			material.SetVector(_Minimum, new Vector4(Minimum, Minimum, Minimum, Minimum));
		}

		public override void Pool()
		{
			pool.Push(this);
		}

		public override void Transform(Matrix4x4 posMatrix, Matrix4x4 rotMatrix, Matrix4x4 rotMatrix2)
		{
		}

		public override CwCommand SpawnCopy()
		{
			var command = SpawnCopy(pool);

			command.Blend   = Blend;
			command.Texture = Texture;
			command.Color   = Color;
			command.Opacity = Opacity;
			command.Minimum = Minimum;

			return command;
		}

		public override void Apply(CwPaintableTexture paintableTexture)
		{
			base.Apply(paintableTexture);

			if (Blend.Index == CwBlendMode.REPLACE_ORIGINAL || Blend.Index == CwBlendMode.NORMAL_REPLACE_ORIGINAL)
			{
				Blend.Color   = paintableTexture.Color;
				Blend.Texture = paintableTexture.Texture;
			}
		}

		public Material SetMaterial(CwBlendMode blendMode, Texture texture, Color color, float opacity, float minimum)
		{
			Blend    = blendMode;
			Material = new CwHashedMaterial(cachedMaterial, cachedMaterialHash);
			Pass     = blendMode;
			Texture  = texture;
			Color    = color;
			Opacity  = opacity;
			Minimum  = minimum;

			return cachedMaterial;
		}
	}
}