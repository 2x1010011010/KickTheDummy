using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to define a set of <b>CwPaintableTexture</b> and <b>CwMaterial</b> components that are configured for a specific set of Materials.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPreset")]
	[AddComponentMenu("")]
	public class CwPreset : MonoBehaviour
	{
		/// <summary>This allows you to name this preset.
		/// None/null = The GameObject name will be used.</summary>
		public string Title { set { title = value; } get { return title; } } [SerializeField] private string title;

		/// <summary>This preset is designed to work with the specified shaders.</summary>
		public List<string> ShaderPaths { get { if (shaderPaths == null) shaderPaths = new List<string>(); return shaderPaths; } } [SerializeField] private List<string> shaderPaths;

		private static List<CwPreset> cachedPresets;

#if UNITY_EDITOR
		/// <summary>This gives you a list of all presets in the project.
		/// NOTE: This is editor-only.</summary>
		public static List<CwPreset> CachedPresets
		{
			get
			{
				if (cachedPresets == null)
				{
					cachedPresets = new List<CwPreset>();

					var scriptGuid  = CwCommon.FindScriptGUID<CwPreset>();

					if (scriptGuid != null)
					{
						foreach (var prefabGuid in UnityEditor.AssetDatabase.FindAssets("t:prefab"))
						{
							var preset = CwCommon.LoadPrefabIfItContainsScriptGUID<CwPreset>(prefabGuid, scriptGuid);

							if (preset != null)
							{
								cachedPresets.Add(preset);
							}
						}
					}
				}

				return cachedPresets;
			}
		}

		public static void ClearCache()
		{
			cachedPresets = null;
		}

		public string FinalName
		{
			get
			{
				return string.IsNullOrEmpty(title) == false ? title : name;
			}
		}

		public bool CanAddTo(Renderer root)
		{
			if (root != null)
			{
				var model = GetComponent<CwModel>();

                if (model != null)
                {
                    if (root.GetType() == model.CachedRenderer.GetType())
					{
						var sm = root.sharedMaterial;

						if (sm != null)
						{
							var s = sm.shader;

							if (s != null && shaderPaths.Contains(s.name) == true)
							{
								return true;
							}
						}
					}
                }
            }

			return false;
		}

		/// <summary>This method applies the preset components to the specified paintable.
		/// NOTE: This is editor-only.</summary>
		public void AddTo(Renderer root)
		{
			// Copy model
			var model =	GetComponent<CwModel>();

			if (UnityEditorInternal.ComponentUtility.CopyComponent(model) == true)
			{
				var newModel = root.gameObject.AddComponent(model.GetType());

				UnityEditorInternal.ComponentUtility.PasteComponentValues(newModel);
			}

			// Copy paintable textures
			foreach (var paintableTexture in GetComponents<CwPaintableTexture>())
			{
				if (UnityEditorInternal.ComponentUtility.CopyComponent(paintableTexture) == true)
				{
					var newPaintableTexture = root.gameObject.AddComponent(paintableTexture.GetType());

					UnityEditorInternal.ComponentUtility.PasteComponentValues(newPaintableTexture);
				}
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwPreset;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPreset_Editor : CwEditor
	{
		private List<string> shaderNames = new List<string>();

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (CwPreset.CachedPresets.Contains(tgt) == false && CwHelper.IsAsset(tgt) == true)
			{
				CwPreset.CachedPresets.Add(tgt);
			}

			Info("After you create this preset, you can open the 'Window/CW/Paintable Objects' window and use it to making paintable objects.");

			if (Any(tgts, t => t.GetComponent<Renderer>() == null))
			{
				Error("You must add a ___Renderer component to this preset.");
			}

			if (Any(tgts, t => t.GetComponent<CwModel>() == null))
			{
				Error("You must add a CwPaintable___ component to this preset.");
			}

			if (Any(tgts, t => t.GetComponent<CwPaintableTexture>() == null))
			{
				Error("You must add a CwPaintable___Texture component to this preset.");
			}

			if (Any(tgts, t => t.GetComponent<CwPaintableTexture>() == null))
			{
				Error("You must add ShaderPaths to this preset. After adding and configuring CwPaintable___Texture components, you can click the Find Shaders button to see a list of shaders that may be applicable to this preset.");
			}

			Draw("title", "This allows you to name this preset.\n\nNone/null = The GameObject name will be used.");
			BeginError(Any(tgts, t => t.ShaderPaths.Count == 0));
				Draw("shaderPaths", "This preset is designed to work with the specified shaders.");
			EndError();

			if (tgts.Length == 1)
			{
				if (Button("Find Shaders") == true)
				{
					shaderNames.Clear();

					var paintableTextures = tgt.GetComponentsInChildren<CwPaintableTexture>();

					foreach (var shaderInfo in ShaderUtil.GetAllShaderInfo())
					{
						if (shaderInfo.name.StartsWith("Hidden/") == false && tgt.ShaderPaths.Contains(shaderInfo.name) == false)
						{
							var shader = Shader.Find(shaderInfo.name);

							if (shader != null)
							{
								foreach (var paintableTexture in paintableTextures)
								{
									if (ShaderHasTexture(shader, paintableTexture.Slot.Name) == false)
									{
										goto NextShader;
									}
								}

								shaderNames.Add(shaderInfo.name);
							}

							NextShader:
							continue;
						}
					}
				}

				var removeShaderName = default(string);

				foreach (var shaderName in shaderNames)
				{
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(shaderName);
						if (GUILayout.Button("+", GUILayout.Width(20)) == true)
						{
							Each(tgts, t => t.ShaderPaths.Add(shaderName), true, true);

							removeShaderName = shaderName;
						}
					EditorGUILayout.EndHorizontal();
				}

				if (removeShaderName != null)
				{
					shaderNames.Remove(removeShaderName);
				}
			}

			if (Button("Clear Cached Presets") == true)
			{
				CwPreset.ClearCache();
			}
		}

		private static bool ShaderHasTexture(Shader s, string n)
		{
			for (var i = 0; i < s.GetPropertyCount(); i++)
			{
				if (s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture && s.GetPropertyName(i) == n)
				{
					return true;
				}
			}

			return false;
		}

		public static void CreateAsset(System.Action<GameObject> setup)
		{
			var preset = new GameObject("Preset").AddComponent<CwPreset>();
			var guids  = Selection.assetGUIDs;
			var path   = guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;

			if (string.IsNullOrEmpty(path) == true)
			{
				path = "Assets";
			}
			else if (AssetDatabase.IsValidFolder(path) == false)
			{
				path = System.IO.Path.GetDirectoryName(path);
			}

			setup(preset.gameObject);

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewPreset.prefab");
			var asset            = PrefabUtility.SaveAsPrefabAsset(preset.gameObject, assetPathAndName);

			DestroyImmediate(preset.gameObject);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset; EditorGUIUtility.PingObject(asset);
		}
	}
}
#endif