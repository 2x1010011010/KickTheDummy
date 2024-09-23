using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component fills the attached UI Image based on the total amount of pixels that have been painted in the specified <b>CwColorCounter</b> components.</summary>
	[RequireComponent(typeof(Image))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwColorCounterFill")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Color Counter Fill")]
	public class CwColorCounterFill : MonoBehaviour
	{
		/// <summary>This allows you to specify the counters that will be used.
		/// Zero = All active and enabled counters in the scene.</summary>
		public List<CwColorCounter> Counters { get { if (counters == null) counters = new List<CwColorCounter>(); return counters; } } [SerializeField] private List<CwColorCounter> counters;

		/// <summary>This allows you to set which color will be handled by this component.</summary>
		public CwColor Color { set { color = value; } get { return color; } } [SerializeField] private CwColor color;

		/// <summary>Inverse the fill?</summary>
		public bool Inverse { set { inverse = value; } get { return inverse; } } [SerializeField] private bool inverse;

		[System.NonSerialized]
		private Image cachedImage;

		protected virtual void OnEnable()
		{
			cachedImage = GetComponent<Image>();
		}

		protected virtual void Update()
		{
			var finalCounters = counters.Count > 0 ? counters : null;
			var ratio         = CwColorCounter.GetRatio(color, finalCounters);

			if (inverse == true)
			{
				ratio = 1.0f - ratio;
			}

			cachedImage.fillAmount = Mathf.Clamp01(ratio);
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwColorCounterFill;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwColorCounterFill_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("counters", "This allows you to specify the counters that will be used.\n\nZero = All active and enabled counters in the scene.");

			Separator();

			BeginError(Any(tgts, t => t.Color == null));
				Draw("color", "This allows you to set which color will be handled by this component.");
			EndError();
			Draw("inverse", "Inverse the fill?");
		}
	}
}
#endif