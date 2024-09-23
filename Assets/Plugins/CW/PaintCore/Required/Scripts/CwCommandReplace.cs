using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This class manages the replace painting command.</summary>
	public class CwCommandReplace : CwCommand
	{
		public CwHashedTexture Texture;
		public Color           Color;

		public static CwCommandReplace Instance = new CwCommandReplace();

		private static Stack<CwCommandReplace> pool = new Stack<CwCommandReplace>();
		
		private static Material cachedMaterial;

		private static int cachedMaterialHash;

		public override bool RequireMesh { get { return false; } }

		private static int _Texture = Shader.PropertyToID("_Texture");
		private static int _Color   = Shader.PropertyToID("_Color");

		static CwCommandReplace()
		{
			BuildMaterial(ref cachedMaterial, ref cachedMaterialHash, "Hidden/PaintCore/CwReplace");
		}

		public static void Blit(RenderTexture renderTexture, Texture texture, Color tint)
		{
			var material = Instance.SetMaterial(texture, tint);

			Instance.Apply(material);

			CwCommon.Blit(renderTexture, material, Instance.Pass);
		}

		public static void BlitFast(RenderTexture renderTexture, Texture texture, Color tint)
		{
			var material = Instance.SetMaterial(texture, tint);

			Instance.Apply(material);

			Graphics.Blit(default(Texture), renderTexture, material);
		}

		public override void Apply(Material material)
		{
			base.Apply(material);

			material.SetTexture(_Texture, Texture);
			material.SetColor(_Color, CwHelper.ToLinear(Color));
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

			command.Texture = Texture;
			command.Color   = Color;

			return command;
		}

		public Material SetMaterial(Texture texture, Color color)
		{
			Material = new CwHashedMaterial(cachedMaterial, cachedMaterialHash);
			Texture  = texture;
			Color    = color;

			return cachedMaterial;
		}
	}
}