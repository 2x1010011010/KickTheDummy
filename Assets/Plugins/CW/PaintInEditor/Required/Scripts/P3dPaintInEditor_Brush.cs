#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PaintIn3D
{
	public partial class P3dPaintInEditor
	{
		private Vector2 brushScrollPosition;

		private List<IBrowsable> brushItems = new List<IBrowsable>();

		private void LoadBrush(P3dBrush prefab)
		{
			Settings.Brush = prefab.Data.GetClone();

			if (EditorApplication.isPlaying == true)
			{
				if (materialInstance != null && toolInstance != null)
				{
					materialInstance.transform.SetParent(toolInstance.transform, false);
				}
			}

			Repaint();
		}

		private void DrawBrush()
		{
			brushItems.Clear();

			foreach (var cachedBrush in P3dBrush.CachedBrushes)
			{
				if (cachedBrush != null)
				{
					brushItems.Add(cachedBrush);
				}
			}

			brushScrollPosition = GUILayout.BeginScrollView(brushScrollPosition, GUILayout.ExpandHeight(true));
				var selected = DrawBrowser(brushItems, null);

				if (selected != null)
				{
					LoadBrush((P3dBrush)selected); selectingBrush = false;
				}
			GUILayout.EndScrollView();

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				Settings.IconSize = EditorGUILayout.IntSlider(Settings.IconSize, 32, 256);

				EditorGUILayout.Separator();

				if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
				{
					P3dBrush.ClearCache(); AssetDatabase.Refresh();
				}

				if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
				{
					selectingBrush = false;
				}
			EditorGUILayout.EndHorizontal();
		}
	}
}
#endif