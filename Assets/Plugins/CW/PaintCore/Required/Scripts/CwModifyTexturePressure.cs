using UnityEngine;
using CW.Common;
using UnityEngine.Scripting.APIUpdating;

namespace PaintCore
{
	/// <summary>This class allows you to change the painting texture of the attached component (e.g. CwPaintDecal) based on the paint pressure.</summary>
	[System.Serializable]
	[MovedFrom(true, "PaintIn3D", "PaintIn3D", "P3dModifyTexturePressure")]
	public class CwModifyTexturePressure : CwModifier
	{
		public static string Group = "Texture";

		public static string Title = "Pressure";

		/// <summary>The painting texture will be changed to this.</summary>
		public Texture Texture { set { texture = value; } get { return texture; } } [SerializeField] private Texture texture;

		/// <summary>The paint pressure must be at least this value.</summary>
		public float PressureMin { set { pressureMin = value; } get { return pressureMin; } } [SerializeField] private float pressureMin = 0.5f;

		/// <summary>The paint pressure must be at most this value.</summary>
		public float PressureMax { set { pressureMax = value; } get { return pressureMax; } } [SerializeField] private float pressureMax = 1.0f;

		protected override void OnModifyTexture(ref Texture currentTexture, float pressure)
		{
			if (pressure >= pressureMin && pressure <= pressureMax)
			{
				currentTexture = texture;
			}
		}

#if UNITY_EDITOR
		public override void DrawEditorLayout()
		{
			texture = (Texture)UnityEditor.EditorGUI.ObjectField(CwEditor.Reserve(18), new GUIContent("Texture", "The painting texture will be changed to this."), texture, typeof(Texture), true);
			pressureMin = UnityEditor.EditorGUILayout.FloatField(new GUIContent("Pressure Min", "The paint pressure must be at least this value."), pressureMin);
			pressureMax = UnityEditor.EditorGUILayout.FloatField(new GUIContent("Pressure Max", "The paint pressure must be at most this value."), pressureMax);
		}
#endif
	}
}