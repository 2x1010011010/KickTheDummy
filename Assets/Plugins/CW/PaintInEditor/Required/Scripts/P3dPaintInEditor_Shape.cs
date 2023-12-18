#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PaintIn3D
{
	public partial class P3dPaintInEditor
	{
		private Vector2 shapeScrollPosition;

		private List<IBrowsable> shapeItems = new List<IBrowsable>();

		private void ClearShape()
		{
		}

		private void LoadShape(P3dShape prefab)
		{
			Settings.Brush.CurrentShape = prefab;

			Repaint();
		}

		private void DrawShape()
		{
			shapeItems.Clear();

			foreach (var cachedShape in P3dShape.CachedShapes)
			{
				if (cachedShape != null)
				{
					shapeItems.Add(cachedShape);
				}
			}

			shapeScrollPosition = GUILayout.BeginScrollView(shapeScrollPosition, GUILayout.ExpandHeight(true));
				var selected = DrawBrowser(shapeItems, Settings.Brush.CurrentShape);

				if (selected != null)
				{
					LoadShape((P3dShape)selected); selectingShape = false;
				}
			GUILayout.EndScrollView();

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				Settings.IconSize = EditorGUILayout.IntSlider(Settings.IconSize, 32, 256);

				EditorGUILayout.Separator();

				if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
				{
					P3dShape.ClearCache(); AssetDatabase.Refresh();
				}

				if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) == true)
				{
					selectingShape = false;
				}
			EditorGUILayout.EndHorizontal();
		}
	}
}
#endif