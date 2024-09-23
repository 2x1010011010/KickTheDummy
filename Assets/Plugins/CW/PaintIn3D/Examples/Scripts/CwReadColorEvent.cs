using UnityEngine;
using UnityEngine.Events;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This component allows you to perform an event when the attached <b>CwReadColor</b> component reads a specific color.</summary>
	[RequireComponent(typeof(CwReadColor))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwReadColorEvent")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Read Color Event")]
	public class CwReadColorEvent : MonoBehaviour
	{
		[System.Serializable] public class ColorEvent : UnityEvent<Color> {}

		/// <summary>This color we want to detect.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The RGBA values must be within this range of a color for it to be counted.</summary>
		public float Threshold { set { threshold = value; } get { return threshold; } } [Range(0.0f, 1.0f)] [SerializeField] private float threshold = 0.1f;

		/// <summary>When the expected color is read, this event will be invoked.
		/// Color = The expected color.</summary>
		public ColorEvent OnColor { get { if (onColor == null) onColor = new ColorEvent(); return onColor; } } [SerializeField] private ColorEvent onColor;

		[System.NonSerialized]
		private CwReadColor cachedReadColor;

		protected virtual void OnEnable()
		{
			cachedReadColor = GetComponent<CwReadColor>();

			cachedReadColor.OnColor.AddListener(HandleColor);
		}

		protected virtual void OnDisable()
		{
			cachedReadColor.OnColor.RemoveListener(HandleColor);
		}

		private void HandleColor(Color read)
		{
			var color32     = (Color32)color;
			var read32      = (Color32)read;
			var threshold32 = (int)(threshold * 255.0f);
			var distance    = 0;

			distance += System.Math.Abs(color32.r - read32.r);
			distance += System.Math.Abs(color32.g - read32.g);
			distance += System.Math.Abs(color32.b - read32.b);
			distance += System.Math.Abs(color32.a - read32.a);

			if (distance <= threshold32)
			{
				if (onColor != null)
				{
					onColor.Invoke(color);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwReadColorEvent;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwReadColorEvent_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("color", "This color we want to detect.");
			Draw("threshold", "The RGBA values must be within this range of a color for it to be counted.");

			Separator();

			Draw("onColor");
		}
	}
}
#endif