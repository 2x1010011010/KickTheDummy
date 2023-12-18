using UnityEngine;

namespace PaintIn3D
{
	[System.Serializable]
	public class P3dBrushData
	{
		public P3dTool CurrentTool;

		public P3dMaterial CurrentMaterial;

		public P3dShape CurrentShape;

		public bool OverrideRadius = true;

		public float Radius = 1.0f;

		public bool OverrideColor = true;

		public Color Color = Color.red;

		public bool OverrideAngle;

		public float Angle;

		public bool OverrideTiling;

		public float Tiling = 1.0f;

		public bool OverrideNormal;

		public float NormalFront = 1.0f;

		public float NormalBack = 0.0f;

		public float NormalFade = 0.01f;

		public bool OverrideModifiers;

		public P3dModifierList Modifiers = new P3dModifierList();

		public P3dBrushData GetClone()
		{
			return (P3dBrushData)this.MemberwiseClone();
		}
	}
}