using CW.Common;
using UnityEngine;
using System.Collections.Generic;

namespace PaintIn3D
{
	/// <summary>This class allows you to easily dilate the specified mesh texture.</summary>
	public static class CwDilate
	{
		private static int _CwCoord   = Shader.PropertyToID("_CwCoord");
		private static int _CwTexure  = Shader.PropertyToID("_CwTexure");
		private static int _CwLookup  = Shader.PropertyToID("_CwLookup");
		private static int _CwOffsets = Shader.PropertyToID("_CwOffsets");
		private static int _CwSize    = Shader.PropertyToID("_CwSize");
		private static int _CwSamples = Shader.PropertyToID("_CwSamples");
		private static int _CwScale   = Shader.PropertyToID("_CwScale");

		private static Material dilateMaterial;

		private static Vector4[] offsets;

		static CwDilate()
		{
			var directions  = new Vector2[8] { new Vector2(0,1), new Vector2(0,-1), new Vector2(1,0), new Vector2(-1,0), new Vector2(1,1),new Vector2(-1,-1),new Vector2(-1,1),new Vector2(1,-1) };
			var tempOffsets = new List<Vector3>();
			var range       = 32;

			tempOffsets.Add(new Vector3(0, 0, 0));

			for (var r = 1; r <= range; r++)
			{
				for (var d = 0; d < 8; d++)
				{
					var dir = directions[d] * r;
					var mag = dir.magnitude;

					if (dir.magnitude <= range)
					{
						tempOffsets.Add(new Vector3(dir.x, dir.y, mag));
					}
				}
			}

			tempOffsets.Sort((a, b) => a.z.CompareTo(b.z));

			offsets = tempOffsets.ConvertAll(a => new Vector4(a.x, a.y)).ToArray();
		}

		public static void Dilate(RenderTexture texture, Mesh mesh, int channel, int highQualitySteps)
		{
			if (dilateMaterial == null)
			{
				dilateMaterial = CwHelper.CreateTempMaterial("Dilate", "Hidden/PaintIn3D/CwDilate");
			}

			var oldActive = RenderTexture.active;
			var format    = highQualitySteps > 0 ? RenderTextureFormat.ARGBInt : RenderTextureFormat.R8;
			var mask      = PaintCore.CwCommon.GetRenderTexture(new RenderTextureDescriptor(texture.width, texture.height, format));
			var deltas    = PaintCore.CwCommon.GetRenderTexture(new RenderTextureDescriptor(texture.width, texture.height, format));
			var swap      = PaintCore.CwCommon.GetRenderTexture(texture);

			mask.filterMode = FilterMode.Point;
			deltas.filterMode = FilterMode.Point;

			// Pass 0
			RenderTexture.active = mask;

			dilateMaterial.SetVector(_CwCoord, PaintCore.CwCommon.IndexToVector(channel));
			dilateMaterial.SetVector(_CwSize, new Vector2(mask.width, mask.height));
			dilateMaterial.SetVectorArray(_CwOffsets, offsets);
			dilateMaterial.SetInt(_CwSamples, offsets.Length);
			dilateMaterial.SetVector(_CwScale, new Vector2(1.0f / mask.width, 1.0f / mask.height));

			if (dilateMaterial.SetPass(0) == true)
			{
				Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
			}

			if (highQualitySteps > 0)
			{
				var texA = deltas;
				var texB = mask;

				for (var i = 0; i < highQualitySteps; i++)
				{
					var t = texA; texA = texB; texB = t;

					dilateMaterial.SetTexture(_CwTexure, texA);

					Graphics.Blit(null, texB, dilateMaterial, 3);
				}

				dilateMaterial.SetTexture(_CwTexure, texture);
				dilateMaterial.SetTexture(_CwLookup, texB);

				Graphics.Blit(null, swap, dilateMaterial, 4);
			}
			else
			{
				dilateMaterial.SetTexture(_CwTexure, mask);

				Graphics.Blit(null, deltas, dilateMaterial, 1);

				dilateMaterial.SetTexture(_CwTexure, texture);
				dilateMaterial.SetTexture(_CwLookup, deltas);

				Graphics.Blit(null, swap, dilateMaterial, 2);
			}

			// Swap
			Graphics.Blit(swap, texture);
			
			PaintCore.CwCommon.ReleaseRenderTexture(mask);
			PaintCore.CwCommon.ReleaseRenderTexture(deltas);
			PaintCore.CwCommon.ReleaseRenderTexture(swap);

			RenderTexture.active = oldActive;
		}
	}
}