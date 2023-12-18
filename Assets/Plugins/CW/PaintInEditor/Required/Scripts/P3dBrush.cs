using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace PaintIn3D
{
	/// <summary>This component can be used to create brush prefabs for in-editor painting. These will automatically appear in the Paint tab's Brushes list.</summary>
	[HelpURL(P3dCommon.HelpUrlPrefix + "P3dBrush")]
	[AddComponentMenu(P3dCommon.ComponentMenuPrefix + "Brush")]
	public class P3dBrush : MonoBehaviour, IBrowsable
	{
		public string Category { set { category = value; } get { return category; } } [SerializeField] private string category;

		public Texture2D Icon { set { icon = value; } get { return icon; } } [SerializeField] private Texture2D icon;

		public P3dBrushData Data { set { data = value; } get { return data; } } [SerializeField] private P3dBrushData data;

		private static List<P3dBrush> cachedBrushes;

		public static List<P3dBrush> CachedBrushes
		{
			get
			{
				if (cachedBrushes == null)
				{
					cachedBrushes = new List<P3dBrush>();
#if UNITY_EDITOR
					var scriptGuid = P3dCommon.FindScriptGUID<P3dBrush>();

					if (scriptGuid != null)
					{
						foreach (var prefabGuid in UnityEditor.AssetDatabase.FindAssets("t:prefab"))
						{
							var brush = P3dCommon.LoadPrefabIfItContainsScriptGUID<P3dBrush>(prefabGuid, scriptGuid);

							if (brush != null)
							{
								cachedBrushes.Add(brush);
							}
						}
					}
#endif
				}

				return cachedBrushes;
			}
		}

		public static void ClearCache()
		{
			cachedBrushes = null;
		}

		public string GetCategory()
		{
			return category;
		}

		public string GetTitle()
		{
			return name;
		}

		public Texture2D GetIcon()
		{
			return icon;
		}

		public Object GetObject()
		{
			return this;
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = P3dBrush;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class P3dBrush_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (P3dBrush.CachedBrushes.Contains(tgt) == false && CwHelper.IsAsset(tgt) == true)
			{
				P3dBrush.CachedBrushes.Add(tgt);
			}

			Draw("category");
			Draw("icon");
			Draw("data");

			Separator();

			if (tgts.Length == 1)
			{
				BeginDisabled(EditorWindow.HasOpenInstances<P3dPaintInEditor>() == false);
					if (Button("Override From Paint in Editor Window") == true)
					{
						Each(tgts, (t) => t.Data = P3dPaintInEditor.Settings.Brush, true);
					}
				EndDisabled();
			}
		}

		[MenuItem("Assets/Create/Paint in 3D/Brush")]
		private static void CreateAsset()
		{
			var brush = new GameObject("Brush").AddComponent<P3dBrush>();
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

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewBrush.prefab");

			var asset = PrefabUtility.SaveAsPrefabAsset(brush.gameObject, assetPathAndName);

			DestroyImmediate(brush.gameObject);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset; EditorGUIUtility.PingObject(asset);
		}
	}
}
#endif