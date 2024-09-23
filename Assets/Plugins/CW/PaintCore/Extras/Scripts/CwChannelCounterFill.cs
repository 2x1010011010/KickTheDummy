using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component fills the attached UI Image based on the total amount of opaque pixels that have been painted in all active and enabled <b>CwChannelCounter</b> components in the scene.</summary>
	[RequireComponent(typeof(Image))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwChannelCounterFill")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Channel Counter Fill")]
	public class CwChannelCounterFill : MonoBehaviour
	{
		public enum ChannelType
		{
			Red,
			Green,
			Blue,
			Alpha
		}

		/// <summary>This allows you to specify the counters that will be used.
		/// Zero = All active and enabled counters in the scene.</summary>
		public List<CwChannelCounter> Counters { get { if (counters == null) counters = new List<CwChannelCounter>(); return counters; } } [SerializeField] private List<CwChannelCounter> counters;

		/// <summary>This allows you to choose which channel will be output to the UI Image.</summary>
		public ChannelType Channel { set { channel = value; } get { return channel; } } [SerializeField] private ChannelType channel;

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
			var ratio         = 0.0f;

			switch (channel)
			{
				case ChannelType.Red:   ratio = CwChannelCounter.GetRatioR(finalCounters); break;
				case ChannelType.Green: ratio = CwChannelCounter.GetRatioG(finalCounters); break;
				case ChannelType.Blue:  ratio = CwChannelCounter.GetRatioB(finalCounters); break;
				case ChannelType.Alpha: ratio = CwChannelCounter.GetRatioA(finalCounters); break;
			}

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
	using TARGET = CwChannelCounterFill;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwChannelCounterFill_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("counters", "This allows you to specify the counters that will be used.\n\nZero = All active and enabled counters in the scene.");

			Separator();

			Draw("channel", "This allows you to choose which channel will be output to the UI Image.");
			Draw("inverse", "Inverse the fill?");
		}
	}
}
#endif