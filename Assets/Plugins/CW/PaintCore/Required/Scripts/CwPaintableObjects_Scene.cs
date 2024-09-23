#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CW.Common;

namespace PaintCore
{
	public partial class CwPaintableObjects
	{
		private Vector2 sceneScrollPosition;

		private static HashSet<Renderer> roots = new HashSet<Renderer>();

		private static List<Material> tempMaterials = new List<Material>();

		private static void RunRoots()
		{
			roots.Clear();

			foreach (var transform in Selection.transforms)
			{
				RunRoots(transform);
			}

			foreach (var model in CwHelper.FindObjectsByType<CwModel>())
			{
				if (roots.Contains(model.CachedRenderer) == false)
				{
					roots.Add(model.CachedRenderer);
				}
			}
		}

		private static void RunRoots(Transform t)
		{
			var r = t.GetComponent<Renderer>();

			if (r != null)
			{
				roots.Add(r);
			}

			foreach (Transform child in t)
			{
				RunRoots(child);
			}
		}

		private void DrawMakePaintable(Renderer root)
		{
			var menu = new GenericMenu();

			foreach (var cachedPreset in CwPreset.CachedPresets)
			{
				if (cachedPreset != null)
				{
					var preset = cachedPreset;

					if (preset.CanAddTo(root) == true)
					{
						menu.AddItem(new GUIContent(preset.FinalName), false, () => preset.AddTo(root));
					}
				}
			}

			if (menu.GetItemCount() == 0)
			{
				var shaderName = "<MISSING>";
				var material   = root.sharedMaterial;

				if (material != null)
				{
					var shader = material.shader;

					if (shader != null)
					{
						shaderName = shader.name;
					}
				}

				menu.AddDisabledItem(new GUIContent("Failed to find any presets. See the console for more info."));

				Debug.LogWarning("You tried to make the '" + root + "' Renderer paintable, but no presets were found for it.\nIts first Material uses the '" + shaderName + "' shader.\nTo make a preset for this, you can create a new prefab with the CwPreset component, and add this shader in the ShaderPaths list.", root);
			}

			menu.ShowAsContext();
		}

		private void DoExportMaterial(Material material, int materialIndex, CwPaintableTexture[] paintableTextures)
		{
			var path = AssetDatabase.GetAssetPath(material);
			var dir  = string.IsNullOrEmpty(path) == false ? System.IO.Path.GetDirectoryName(path) : "Assets";

			path = EditorUtility.SaveFilePanelInProject("Export Material & Textures", name, "mat", "Export Your Material and Textures", dir);

			if (string.IsNullOrEmpty(path) == false)
			{
				Undo.RecordObjects(paintableTextures, "Export Material & Textures");

				var clone = Instantiate(material);

				AssetDatabase.CreateAsset(clone, path);

				foreach (var paintableTexture in paintableTextures)
				{
					if (paintableTexture.Slot.Index == materialIndex)
					{
						var finalPath = System.IO.Path.GetDirectoryName(path) + "/" + System.IO.Path.GetFileNameWithoutExtension(path) + paintableTexture.Slot.Name + "." + GetExtension(Settings.DefaultTextureFormat);

						System.IO.File.WriteAllBytes(finalPath, GetData(paintableTexture, Settings.DefaultTextureFormat));

						AssetDatabase.ImportAsset(finalPath);

						paintableTexture.Output = AssetDatabase.AssetPathToGUID(finalPath);

						clone.SetTexture(paintableTexture.Slot.Name, AssetDatabase.LoadAssetAtPath<Texture>(finalPath));
					}
				}

				EditorUtility.SetDirty(this);
			}
		}

		private void DoExportTexture(CwPaintableTexture paintableTexture)
		{
			var path = AssetDatabase.GUIDToAssetPath(paintableTexture.Output);
			var name = paintableTexture.name + "_" + paintableTexture.Slot.Name;
			var dir  = string.IsNullOrEmpty(path) == false ? System.IO.Path.GetDirectoryName(path) : "Assets";

			if (string.IsNullOrEmpty(path) == false)
			{
				name = System.IO.Path.GetFileNameWithoutExtension(path);
			}

			path = EditorUtility.SaveFilePanelInProject("Export Texture", name, GetExtension(Settings.DefaultTextureFormat), "Export Your Texture", dir);

			if (string.IsNullOrEmpty(path) == false)
			{
				System.IO.File.WriteAllBytes(path, GetData(paintableTexture, Settings.DefaultTextureFormat));

				AssetDatabase.ImportAsset(path);

				Undo.RecordObject(paintableTexture, "Output Changed");

				paintableTexture.Output = AssetDatabase.AssetPathToGUID(path);

				EditorUtility.SetDirty(this);
			}
		}

		private void DrawMaterials(CwModel model, CwPaintableTexture[] paintableTextures)
		{
			model.CachedRenderer.GetSharedMaterials(tempMaterials);

			for (var i = 0; i < tempMaterials.Count; i++)
			{
				var material = tempMaterials[i];

				EditorGUILayout.BeginHorizontal();
					EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.ObjectField(GUIContent.none, material, typeof(Material), true, GUILayout.MinWidth(10));
					EditorGUI.EndDisabledGroup();

					if (material != null && paintableTextures.Length > 0)
					{
						if (GUILayout.Button(new GUIContent("Export", "Export this material and all its textures to your project as assets?"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
						{
							DoExportMaterial(material, i, paintableTextures);
						}
					}
				EditorGUILayout.EndHorizontal();

				foreach (var paintableTexture in paintableTextures)
				{
					if (paintableTexture.Model == model && paintableTexture.Slot.Index == i)
					{
						CwEditor.BeginIndent();
							EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField(paintableTexture.Slot.GetTitle(material));
								if (GUILayout.Button(new GUIContent("Export", "Export this texture to the project as an asset?"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
								{
									DoExportTexture(paintableTexture);
								}
							EditorGUILayout.EndHorizontal();
						CwEditor.EndIndent();
					}
				}
			}
		}

		private void DrawObjectsTab(CwPaintableTexture[] paintableTextures)
		{
			RunRoots();

			var modelCount  = 0;
			var removeModel = default(CwModel);

			EditorGUILayout.Separator();

			sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition, GUILayout.ExpandHeight(true));
				foreach (var root in roots)
				{
					var model = root.GetComponent<CwModel>();

					EditorGUILayout.BeginHorizontal();
						EditorGUI.BeginDisabledGroup(true);
							EditorGUILayout.ObjectField(GUIContent.none, root.gameObject, typeof(GameObject), true, GUILayout.MinWidth(10));
						EditorGUI.EndDisabledGroup();
						
						if (model != null)
						{
							modelCount += 1;

							if (GUILayout.Button(new GUIContent("X", "Remove all paintable components from this GameObject?"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
							{
								if (EditorUtility.DisplayDialog("Are you sure?", "Remove painting components from this GameObject?", "OK") == true)
								{
									removeModel = model;
								}
							}
						}
						else
						{
							if (GUILayout.Button("Make Paintable", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
							{
								DrawMakePaintable(root);
							}
						}
					EditorGUILayout.EndHorizontal();

					if (model != null)
					{
						CwEditor.BeginIndent();
							DrawMaterials(model, paintableTextures);
						CwEditor.EndIndent();
					}
				}
			GUILayout.EndScrollView();

			if (modelCount == 0)
			{
				GUILayout.FlexibleSpace();

				EditorGUILayout.HelpBox("Your scene doesn't contain any paintable objects.", MessageType.Warning);
			}

			if (removeModel != null)
			{
				removeModel.RemoveComponents();
			}
		}

		private string GetExtension(ExportTextureFormat f)
		{
			switch (Settings.DefaultTextureFormat)
			{
				case ExportTextureFormat.PNG: return "png";
				case ExportTextureFormat.TGA: return "tga";
			}

			return null;
		}

		private byte[] GetData(CwPaintableTexture t, string path)
		{
			var f = default(ExportTextureFormat);

			if (path.EndsWith("png", System.StringComparison.InvariantCultureIgnoreCase) == true)
			{
				f = ExportTextureFormat.PNG;
			}
			else if (path.EndsWith("tga", System.StringComparison.InvariantCultureIgnoreCase) == true)
			{
				f = ExportTextureFormat.TGA;
			}

			return GetData(t, f);
		}

		private byte[] GetData(CwPaintableTexture t, ExportTextureFormat f)
		{
			switch (Settings.DefaultTextureFormat)
			{
				case ExportTextureFormat.PNG: return t.GetPngData();
				case ExportTextureFormat.TGA: return t.GetTgaData();
			}

			return null;
		}
	}
}
#endif