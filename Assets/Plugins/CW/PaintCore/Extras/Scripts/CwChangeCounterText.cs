using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component will output the total pixels for the specified team to a UI Text component.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwChangeCounterText")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Change Counter Text")]
	public class CwChangeCounterText : MonoBehaviour
	{
		[System.Serializable] public class StringEvent : UnityEvent<string> {}

		/// <summary>This allows you to specify the counters that will be used.
		/// Zero = All active and enabled counters in the scene.</summary>
		public List<CwChangeCounter> Counters { get { if (counters == null) counters = new List<CwChangeCounter>(); return counters; } } [SerializeField] private List<CwChangeCounter> counters;

		/// <summary>Inverse the <b>Count</b> and <b>Percent</b> values?</summary>
		public bool Inverse { set { inverse = value; } get { return inverse; } } [SerializeField] private bool inverse;

		/// <summary>This allows you to set the amount of decimal places when using the percentage output.</summary>
		public int DecimalPlaces { set { decimalPlaces = value; } get { return decimalPlaces; } } [SerializeField] private int decimalPlaces;

		/// <summary>This allows you to set the format of the team text. You can use the following tokens:
		/// {TOTAL} = Total amount of pixels that can be painted.
		/// {COUNT} = Total amount of pixel that have been painted.
		/// {PERCENT} = Percentage of pixels that have been painted.</summary>
		public string Format { set { format = value; } get { return format; } } [Multiline] [SerializeField] private string format = "{PERCENT}";

		/// <summary>The color count will be output via this event.</summary>
		public StringEvent OnString { get { if (onString == null) onString = new StringEvent(); return onString; } } [SerializeField] private StringEvent onString;

		protected virtual void Update()
		{
			var finalCounters = counters.Count > 0 ? counters : null;
			var total         = CwChangeCounter.GetTotal(finalCounters);
			var count         = CwChangeCounter.GetCount(finalCounters);

			if (inverse == true)
			{
				count = total - count;
			}

			var final   = format;
			var percent = CwCommon.RatioToPercentage(CwHelper.Divide(count, total), decimalPlaces);

			final = final.Replace("{TOTAL}", total.ToString());
			final = final.Replace("{COUNT}", count.ToString());
			final = final.Replace("{PERCENT}", percent.ToString());

			if (onString != null)
			{
				onString.Invoke(final);
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwChangeCounterText;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwChangeCounterText_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("counters", "This allows you to specify the counters that will be used.\n\nZero = All active and enabled counters in the scene.");

			Separator();

			Draw("inverse", "Inverse the Count and Percent values?");
			Draw("decimalPlaces", "This allows you to set the amount of decimal places when using the percentage output.");
			Draw("format", "This allows you to set the format of the team text. You can use the following tokens:\n\n{TOTAL} = Total amount of pixels that can be painted.\n\n{COUNT} = Total amount of pixel that have been painted.\n\n{PERCENT} = Percentage of pixels that have been painted.");

			Separator();

			Draw("onString");
		}
	}
}
#endif