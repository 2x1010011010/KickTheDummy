using UnityEngine;
using UnityEngine.Events;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This component invokes the <b>Action</b> event when this component is enabled.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwActionOnEnable")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Action OnEnable")]
	public class CwActionOnEnable : MonoBehaviour
	{
		/// <summary>The event that will be invoked.</summary>
		public UnityEvent Action { get { if (action == null) action = new UnityEvent(); return action; } } [SerializeField] public UnityEvent action;

		protected virtual void OnEnable()
		{
			if (action != null)
			{
				action.Invoke();
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwActionOnEnable;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwActionOnEnable_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("action");
		}
	}
}
#endif