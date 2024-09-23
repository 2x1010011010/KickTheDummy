using System.Collections.Generic;
using UnityEngine;

namespace PaintCore
{
	/// <summary>This class manages the replace channels painting command.</summary>
	public class CwCommandReplaceChannels : CwCommand
	{
		public CwHashedTexture TextureR;
		public CwHashedTexture TextureG;
		public CwHashedTexture TextureB;
		public CwHashedTexture TextureA;
		public Vector4         ChannelR;
		public Vector4         ChannelG;
		public Vector4         ChannelB;
		public Vector4         ChannelA;

		public static CwCommandReplaceChannels Instance = new CwCommandReplaceChannels();

		private static Stack<CwCommandReplaceChannels> pool = new Stack<CwCommandReplaceChannels>();

		private static Material cachedMaterial;

		private static int cachedMaterialHash;

		public override bool RequireMesh { get { return false; } }

		private static int _TextureR = Shader.PropertyToID("_TextureR");
		private static int _TextureG = Shader.PropertyToID("_TextureG");
		private static int _TextureB = Shader.PropertyToID("_TextureB");
		private static int _TextureA = Shader.PropertyToID("_TextureA");
		private static int _ChannelR = Shader.PropertyToID("_ChannelR");
		private static int _ChannelG = Shader.PropertyToID("_ChannelG");
		private static int _ChannelB = Shader.PropertyToID("_ChannelB");
		private static int _ChannelA = Shader.PropertyToID("_ChannelA");

		static CwCommandReplaceChannels()
		{
			BuildMaterial(ref cachedMaterial, ref cachedMaterialHash, "Hidden/Paint Core/CwReplaceChannels");
		}

		public static void Blit(RenderTexture renderTexture, Texture textureR, Texture textureG, Texture textureB, Texture textureA, Vector4 channelR, Vector4 channelG, Vector4 channelB, Vector4 channelA, Vector4 channels)
		{
			var material = Instance.SetMaterial(textureR, textureG, textureB, textureA, channelR, channelG, channelB, channelA);

			Instance.Apply(material);

			CwCommon.Blit(renderTexture, material, Instance.Pass);
		}

		public static void BlitFast(RenderTexture renderTexture, Texture textureR, Texture textureG, Texture textureB, Texture textureA, Vector4 channelR, Vector4 channelG, Vector4 channelB, Vector4 channelA)
		{
			var material = Instance.SetMaterial(textureR, textureG, textureB, textureA, channelR, channelG, channelB, channelA);

			Instance.Apply(material);

			Graphics.Blit(default(Texture), renderTexture, material);
		}

		public override void Apply(Material material)
		{
			base.Apply(material);

			material.SetTexture(_TextureR, TextureR);
			material.SetTexture(_TextureG, TextureG);
			material.SetTexture(_TextureB, TextureB);
			material.SetTexture(_TextureA, TextureA);
			material.SetVector(_ChannelR, ChannelR);
			material.SetVector(_ChannelG, ChannelG);
			material.SetVector(_ChannelB, ChannelB);
			material.SetVector(_ChannelA, ChannelA);
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

			command.TextureR = TextureR;
			command.TextureG = TextureG;
			command.TextureB = TextureB;
			command.TextureA = TextureA;
			command.ChannelR = ChannelR;
			command.ChannelG = ChannelG;
			command.ChannelB = ChannelB;
			command.ChannelA = ChannelA;

			return command;
		}

		public Material SetMaterial(Texture textureR, Texture textureG, Texture textureB, Texture textureA, Vector4 channelR, Vector4 channelG, Vector4 channelB, Vector4 channelA)
		{
			Material = new CwHashedMaterial(cachedMaterial, cachedMaterialHash);
			TextureR = textureR;
			TextureG = textureG;
			TextureB = textureB;
			TextureA = textureA;
			ChannelR = channelR;
			ChannelG = channelG;
			ChannelB = channelB;
			ChannelA = channelA;

			return cachedMaterial;
		}
	}
}