#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CW.Common;
using System.Collections.Generic;

namespace PaintIn3D
{
	public partial class P3dPaintInEditor
	{
		public enum ExportTextureFormat
		{
			PNG,
			TGA
		}

		[System.Serializable]
		public class SettingsData
		{
			public P3dGroup ColorGroup = 0;

			public int IconSize = 128;

			public P3dBrushData Brush = new P3dBrushData();
		}

		public static SettingsData Settings = new SettingsData();

		private Vector2 configScrollPosition;

		private static void ClearSettings()
		{
			if (EditorPrefs.HasKey("PaintIn3D.Settings") == true)
			{
				EditorPrefs.DeleteKey("PaintIn3D.Settings");

				Settings = new SettingsData();
			}
		}

		private static void SaveSettings()
		{
			EditorPrefs.SetString("PaintIn3D.Settings", EditorJsonUtility.ToJson(Settings));
		}

		private static void LoadSettings()
		{
			if (EditorPrefs.HasKey("PaintIn3D.Settings") == true)
			{
				var json = EditorPrefs.GetString("PaintIn3D.Settings");

				if (string.IsNullOrEmpty(json) == false)
				{
					EditorJsonUtility.FromJsonOverwrite(json, Settings);
				}
			}
		}

		private void DrawConfigTab()
		{
			configScrollPosition = GUILayout.BeginScrollView(configScrollPosition, GUILayout.ExpandHeight(true));
				CwEditor.BeginLabelWidth(100);
					Settings.ColorGroup = EditorGUILayout.IntField("Color Group", Settings.ColorGroup);
					Settings.IconSize = EditorGUILayout.IntSlider("Icon Size", Settings.IconSize, 32, 256);

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Clear Settings") == true)
					{
						if (EditorUtility.DisplayDialog("Are you sure?", "This will reset all editor painting settings to default.", "OK") == true)
						{
							ClearSettings();
						}
					}
				CwEditor.EndLabelWidth();
			GUILayout.EndScrollView();
		}
	}
}
#endif