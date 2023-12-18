#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CW.Common;

namespace PaintIn3D
{
	public partial class P3dPaintInEditor
	{
		private Vector2 paintScrollPosition;

		private bool selectingTool;

		private bool selectingMaterial;

		private bool selectingShape;

		private bool selectingBrush;

		private void DrawPaintTab()
		{
			if (selectingTool == true)
			{
				DrawTool(); return;
			}

			if (selectingMaterial == true)
			{
				DrawMaterial(); return;
			}

			if (selectingShape == true)
			{
				DrawShape(); return;
			}

			if (selectingBrush == true)
			{
				DrawBrush(); return;
			}

			paintScrollPosition = GUILayout.BeginScrollView(paintScrollPosition, GUILayout.ExpandHeight(true));
				DrawTop();

				EditorGUILayout.Separator();

				DrawBrushData(Settings.Brush);
			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			if (Application.isPlaying == false)
			{
				EditorGUILayout.HelpBox("You must enter play mode to begin painting.", MessageType.Warning);
			}
			else
			{
				if (P3dSceneTool.IsActive == true)
				{
					if (P3dSceneTool.LastValid == false)
					{
						EditorGUILayout.HelpBox("You must paint in the Scene view.", MessageType.Warning);
					}

					if (SceneContainsNormalPaintComponent() == true)
					{
						EditorGUILayout.HelpBox("Your scene contains active paint components. These may conflict with the in-editor painting features.", MessageType.Warning);
					}
				}
				else
				{
					if (CwEditor.HelpButton("You must select the Paint in 3D tool from the top left tools menu to paint.", MessageType.Warning, "Select", 50) == true)
					{
						P3dSceneTool.SelectThisTool();
					}
				}
			}

			UpdatePaint();
		}

		private static bool SceneContainsNormalPaintComponent()
		{
			foreach (var paintDecal in CwHelper.FindObjectsByType<P3dPaintDecal>())
			{
				if (paintDecal.GetComponentInParent<P3dTool>() == null)
				{
					return true;
				}
			}

			foreach (var paintDecal in CwHelper.FindObjectsByType<P3dPaintSphere>())
			{
				if (paintDecal.GetComponentInParent<P3dTool>() == null)
				{
					return true;
				}
			}

			return false;
		}

		private void DrawTop()
		{
			var toolIcon      = default(Texture2D);
			var toolTitle     = "None";
			var materialIcon  = default(Texture2D);
			var materialTitle = "None";
			var shapeIcon     = default(Texture2D);
			var shapeTitle    = "None";
			var width         = Mathf.FloorToInt((position.width - 30) / 3);

			if (Settings.Brush.CurrentTool != null)
			{
				toolIcon  = Settings.Brush.CurrentTool.GetIcon();
				toolTitle = Settings.Brush.CurrentTool.GetTitle();
			}

			if (Settings.Brush.CurrentMaterial != null)
			{
				materialIcon  = Settings.Brush.CurrentMaterial.GetIcon();
				materialTitle = Settings.Brush.CurrentMaterial.GetTitle();
			}

			if (Settings.Brush.CurrentShape != null)
			{
				shapeIcon  = Settings.Brush.CurrentShape.GetIcon();
				shapeTitle = Settings.Brush.CurrentShape.GetTitle();
			}

			EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.BeginVertical();
					var rectA = DrawIcon(width, toolIcon, toolTitle, Settings.Brush.CurrentTool != null, "Tool");
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				EditorGUILayout.BeginVertical();
					var rectB = DrawIcon(width, materialIcon, materialTitle, Settings.Brush.CurrentMaterial != null, "Material");
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				EditorGUILayout.BeginVertical();
					var rectC = DrawIcon(width, shapeIcon, shapeTitle, Settings.Brush.CurrentShape != null, "Shape");
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			if (Event.current.type == EventType.MouseDown && rectA.Contains(Event.current.mousePosition) == true)
			{
				if (Event.current.button == 0)
				{
					selectingTool = true;
				}
				else
				{
					CwHelper.SelectAndPing(Settings.Brush.CurrentTool);
				}
			}

			if (Event.current.type == EventType.MouseDown && rectB.Contains(Event.current.mousePosition) == true)
			{
				if (Event.current.button == 0)
				{
					selectingMaterial = true;
				}
				else
				{
					CwHelper.SelectAndPing(Settings.Brush.CurrentMaterial);
				}
			}

			if (Event.current.type == EventType.MouseDown && rectC.Contains(Event.current.mousePosition) == true)
			{
				if (Event.current.button == 0)
				{
					selectingShape = true;
				}
				else
				{
					CwHelper.SelectAndPing(Settings.Brush.CurrentShape);
				}
			}
		}

		private static void DrawBrushData(P3dBrushData brush)
		{
			CwEditor.BeginLabelWidth(100);
				DrawRadius(brush);
			CwEditor.EndLabelWidth();

			EditorGUILayout.Separator();

			CwEditor.BeginLabelWidth(100);
				DrawColor(brush);
			CwEditor.EndLabelWidth();

			EditorGUILayout.Separator();

			CwEditor.BeginLabelWidth(100);
				DrawAngle(brush);
			CwEditor.EndLabelWidth();

			EditorGUILayout.Separator();

			CwEditor.BeginLabelWidth(100);
				DrawTiling(brush);
			CwEditor.EndLabelWidth();

			EditorGUILayout.Separator();

			CwEditor.BeginLabelWidth(100);
				DrawNormal(brush);
			CwEditor.EndLabelWidth();

			EditorGUILayout.Separator();

			CwEditor.BeginLabelWidth(100);
				DrawModifiers(brush);
			CwEditor.EndLabelWidth();
		}

		private static void DrawRadius(P3dBrushData brush)
		{
			brush.OverrideRadius = EditorGUILayout.Toggle("Override Radius", brush.OverrideRadius);

			if (brush.OverrideRadius == true)
			{
				EditorGUI.indentLevel++;
					brush.Radius = LogSlider("Radius", brush.Radius, -4, 4);
				EditorGUI.indentLevel--;
			}
		}

		private static void DrawColor(P3dBrushData brush)
		{
			brush.OverrideColor = EditorGUILayout.Toggle("Override Color", brush.OverrideColor);

			if (brush.OverrideColor == true)
			{
				EditorGUI.indentLevel++;
					brush.Color   = EditorGUILayout.ColorField("Color", brush.Color);
					brush.Color.r = Slider("Red", brush.Color.r, 0.0f, 1.0f);
					brush.Color.g = Slider("Green", brush.Color.g, 0.0f, 1.0f);
					brush.Color.b = Slider("Blue", brush.Color.b, 0.0f, 1.0f);
					brush.Color.a = Slider("Alpha", brush.Color.a, 0.0f, 1.0f);

					float h, s, v; Color.RGBToHSV(brush.Color, out h, out s, out v);

					h = Slider("Hue"       , h, 0.0f, 1.0f);
					s = Slider("Saturation", s, 0.0f, 1.0f);
					v = Slider("Value"     , v, 0.0f, 1.0f);

					var newColor = Color.HSVToRGB(h, s, v);

					brush.Color.r = newColor.r;
					brush.Color.g = newColor.g;
					brush.Color.b = newColor.b;
				EditorGUI.indentLevel--;
			}
		}

		private static void DrawAngle(P3dBrushData brush)
		{
			brush.OverrideAngle = EditorGUILayout.Toggle("Override Angle", brush.OverrideAngle);

			if (brush.OverrideAngle == true)
			{
				EditorGUI.indentLevel++;
					brush.Angle = Slider("Angle", brush.Angle, -180.0f, 180.0f);
				EditorGUI.indentLevel--;
			}
		}

		private static void DrawTiling(P3dBrushData brush)
		{
			brush.OverrideTiling = EditorGUILayout.Toggle("Override Tiling", brush.OverrideTiling);

			if (brush.OverrideTiling == true)
			{
				EditorGUI.indentLevel++;
					brush.Tiling = Slider("Tiling", brush.Tiling, 0.1f, 10.0f);
				EditorGUI.indentLevel--;
			}
		}

		private static void DrawNormal(P3dBrushData brush)
		{
			brush.OverrideNormal = EditorGUILayout.Toggle("Override Normal", brush.OverrideNormal);

			if (brush.OverrideNormal == true)
			{
				EditorGUI.indentLevel++;
					brush.NormalFront = Slider("Front", brush.NormalFront, 0.0f, 2.0f);
					brush.NormalBack  = Slider("Back", brush.NormalBack, 0.0f, 2.0f);
					brush.NormalFade  = Slider("Fade", brush.NormalFade, 0.0f, 0.5f);
				EditorGUI.indentLevel--;
			}
		}

		private static void DrawModifiers(P3dBrushData brush)
		{
			brush.OverrideModifiers = EditorGUILayout.Toggle("Override Modifiers", brush.OverrideModifiers);

			if (brush.OverrideModifiers == true)
			{
				EditorGUI.indentLevel++;
					brush.Modifiers.DrawEditorLayout(true, "Color", "Angle", "Opacity", "Radius", "Hardness", "Texture", "Position");
				EditorGUI.indentLevel--;
			}
		}

		private void DrawPivot(Camera camera, Transform root, Matrix4x4 matrix)
		{
			var meshFilter   = root.GetComponent<MeshFilter>();
			var meshRenderer = root.GetComponent<MeshRenderer>();

			if (meshFilter != null && meshRenderer != null && meshFilter.sharedMesh != null && meshRenderer.sharedMaterial != null)
			{
				Graphics.DrawMesh(meshFilter.sharedMesh, matrix * root.localToWorldMatrix, meshRenderer.sharedMaterial, 0, camera, 0, null, UnityEngine.Rendering.ShadowCastingMode.Off, false);
			}

			foreach (Transform child in root)
			{
				DrawPivot(camera, child, matrix);
			}
		}

		private void UpdatePaint()
		{
			if (toolInstance != null)
			{
				/*
				foreach (var connectablePoint in toolInstance.GetComponentsInChildren<P3dPointConnector>())
				{
					connectablePoint.ClearHitCache();
				}

				foreach (var connectableLine in toolInstance.GetComponentsInChildren<P3dLineConnector>())
				{
					connectableLine.ClearHitCache();
				}
				*/
			}

			if (materialInstance != null)
			{
				foreach (var paintSphere in materialInstance.GetComponentsInChildren<P3dPaintSphere>())
				{
					if (paintSphere.Group == Settings.ColorGroup)
					{
						if (Settings.Brush.OverrideColor == true)
						{
							paintSphere.Color  = Settings.Brush.Color;
						}
					}

					if (Settings.Brush.OverrideRadius == true)
					{
						paintSphere.Radius = Settings.Brush.Radius;
					}
				}

				foreach (var paintDecal in materialInstance.GetComponentsInChildren<P3dPaintDecal>())
				{
					if (paintDecal.Group == Settings.ColorGroup)
					{
						if (Settings.Brush.OverrideColor == true)
						{
							paintDecal.Color = Settings.Brush.Color;
						}
					}

					if (Settings.Brush.OverrideRadius == true)
					{
						paintDecal.Radius = Settings.Brush.Radius;
					}

					if (Settings.Brush.OverrideAngle == true)
					{
						paintDecal.Angle = Settings.Brush.Angle;
					}

					if (Settings.Brush.OverrideTiling == true)
					{
						paintDecal.transform.localScale = Vector3.one * Settings.Brush.Tiling;

						paintDecal.TileTransform = paintDecal.transform;
					}

					if (Settings.Brush.OverrideNormal == true)
					{
						paintDecal.NormalFront = Settings.Brush.NormalFront;
						paintDecal.NormalBack  = Settings.Brush.NormalBack;
						paintDecal.NormalFade  = Settings.Brush.NormalFade;
					}

					if (Settings.Brush.OverrideModifiers == true)
					{
						paintDecal.Modifiers.Instances.Clear();

						foreach (var modifier in Settings.Brush.Modifiers.Instances)
						{
							paintDecal.Modifiers.Instances.Add(modifier);
						}
					}

					paintDecal.Shape        = Settings.Brush.CurrentShape != null ? Settings.Brush.CurrentShape.Icon : null;
					paintDecal.ShapeChannel = P3dChannel.Red;
				}
			}
		}
	}
}
#endif