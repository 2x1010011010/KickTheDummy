#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CW.Common;

namespace PaintCore
{
	/// <summary>This window shows you all paintable objects in the scene, and it also allows you to make objects paintable from all paintable presets in your project.
	/// You can open this window from the <b>Window/CW/Paintable Objects</b> menu.</summary>
	public partial class CwPaintableObjects : EditorWindow
	{
		enum PageType
		{
			Objects,
			Config
		}

		private PageType currentPage;

		[MenuItem("Window/CW/Paintable Objects")]
		public static void OpenWindow()
		{
			GetWindow();
		}

		public static CwPaintableObjects GetWindow()
		{
			return GetWindow<CwPaintableObjects>("Paintable Objects", true);
		}

		protected virtual void OnEnable()
		{
			LoadSettings();
		}

		protected virtual void OnDisable()
		{
			SaveSettings();
		}

		protected virtual void Update()
		{
			Repaint();
		}

		protected virtual void OnGUI()
		{
			CwEditor.ClearStacks();

			var paintableTextures = CwHelper.FindObjectsByType<CwPaintableTexture>();

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				EditorGUI.BeginDisabledGroup(CwStateManager.CanUndo == false);
					if (GUILayout.Button(new GUIContent("↺", "Undo"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
					{
						CwStateManager.UndoAll();
					}
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(CwStateManager.CanRedo == false);
					if (GUILayout.Button(new GUIContent("↻", "Redo"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
					{
						CwStateManager.RedoAll();
					}
				EditorGUI.EndDisabledGroup();

				if (GUILayout.Toggle(currentPage == PageType.Objects, "Objects", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
				{
					currentPage = PageType.Objects;
				}

				if (GUILayout.Toggle(currentPage == PageType.Config, "Config", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
				{
					currentPage = PageType.Config;
				}

				EditorGUILayout.Separator();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			switch (currentPage)
			{
				case PageType.Objects:
				{
					DrawObjectsTab(paintableTextures);
				}
				break;

				case PageType.Config:
				{
					DrawConfigTab();
				}
				break;
			}
		}
	}
}
#endif