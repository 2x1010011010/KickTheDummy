#if UNITY_EDITOR
using CW.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PaintIn3D
{
	/// <summary>This class allows you to remap a texture from one UV layout to a different UV layout.</summary>
	public static class CwRemap
	{
		private static int _CwTexure  = Shader.PropertyToID("_CwTexure");

		private static Material remapMaterial;

		public static Texture2D Remap(Texture2D sourceTexture, Mesh oldMesh, Mesh newMesh, int newSize)
		{
			if (remapMaterial == null)
			{
				remapMaterial = CwHelper.CreateTempMaterial("Dilate", "Hidden/PaintIn3D/CwRemap");
			}

			var oldActive    = RenderTexture.active;
			var desc         = new RenderTextureDescriptor(newSize, newSize, RenderTextureFormat.ARGB32, 0, 0); desc.sRGB = false;
			var canvas       = PaintCore.CwCommon.GetRenderTexture(desc);
			var remapMesh    = GenerateRemapMesh(oldMesh, newMesh);
			var remapTexture = GenerateRemapTexture(sourceTexture);

			remapMaterial.SetTexture(_CwTexure, remapTexture);

			RenderTexture.active = canvas;

			if (remapMaterial.SetPass(0) == true)
			{
				Graphics.DrawMeshNow(remapMesh, Matrix4x4.identity);
			}

			var dilateSteps = Mathf.Max(canvas.width, canvas.height) / 4;

			CwDilate.Dilate(canvas, remapMesh, 0, dilateSteps);

			var generatedTexture = PaintCore.CwCommon.GetReadableCopy(canvas);

			PaintCore.CwCommon.ReleaseRenderTexture(canvas);

			Object.DestroyImmediate(remapMesh);
			Object.DestroyImmediate(remapTexture);

			RenderTexture.active = oldActive;

			return generatedTexture;
		}

		public static void Export(Texture2D remapTexture, string path, Texture2D sourceTexture)
		{
			var extension = System.IO.Path.GetExtension(path);
			var data      = default(byte[]);

			switch (extension)
			{
				case "tga":          data = remapTexture.EncodeToTGA(); break;
				case "png": default: data = remapTexture.EncodeToPNG(); break;
			}

			System.IO.File.WriteAllBytes(path, data);

			AssetDatabase.ImportAsset(path);

			var sourceImporter = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sourceTexture));

			if (sourceImporter != null)
			{
				var importer = (TextureImporter)AssetImporter.GetAtPath(path);
				var settings = new TextureImporterSettings();

				sourceImporter.ReadTextureSettings(settings);

				importer.SetTextureSettings(settings);

				importer.SaveAndReimport();
			}
		}

		private static Mesh GenerateRemapMesh(Mesh oldMesh, Mesh newMesh)
		{
			var oldCoords      = oldMesh.uv;
			var newCoords      = newMesh.uv;
			var remapMesh      = new Mesh();
			var remapVertices  = new List<Vector3>();
			var remapCoords    = new List<Vector4>();
			var remapTriangles = new List<int>();

			for (var s = 0; s < oldMesh.subMeshCount; s++)
			{
				var oldTriangles = oldMesh.GetTriangles(s);
				var newTriangles = newMesh.GetTriangles(s);

				for (var i = 0; i < oldTriangles.Length; i++)
				{
					var oldCoord = oldCoords[oldTriangles[i]];
					var newCoord = newCoords[newTriangles[i]];

					remapVertices.Add(Vector3.zero);
					remapCoords.Add(new Vector4(newCoord.x, newCoord.y, oldCoord.x, oldCoord.y));
					remapTriangles.Add(i);
				}
			}

			remapMesh.SetVertices(remapVertices);
			remapMesh.SetUVs(0, remapCoords);
			remapMesh.SetTriangles(remapTriangles, 0);

			return remapMesh;
		}

		private static Texture2D GenerateRemapTexture(Texture2D oldTexture)
		{
			var path       = AssetDatabase.GetAssetPath(oldTexture);
			var newTexture = new Texture2D(1, 1);

			if (newTexture.LoadImage(System.IO.File.ReadAllBytes(path), false) == true)
			{
				return newTexture;
			}

			Object.DestroyImmediate(newTexture);

			return Object.Instantiate(oldTexture);
		}
	}
}
#endif