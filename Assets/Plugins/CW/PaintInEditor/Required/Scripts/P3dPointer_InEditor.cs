using CW.Common;
using System.Collections.Generic;
using UnityEngine;

namespace PaintIn3D
{
	/// <summary>This component sends pointer information to any <b>P3dHitScreen</b> component, allowing you to paint with the mouse.</summary>
	[RequireComponent(typeof(P3dHitScreenBase))]
	[HelpURL(P3dCommon.HelpUrlPrefix + "P3dPointer_InEditor")]
	[AddComponentMenu("")]
	public class P3dPointer_InEditor : P3dPointer
	{
		/// <summary>If you enable this, then a paint preview will be shown under the mouse as long as the <b>RequiredKey</b> is not pressed.</summary>
		public bool Preview { set { preview = value; } get { return preview; } } [SerializeField] private bool preview;

		private readonly int PREVIEW_FINGER_INDEX = -1;

		private readonly int PAINT_FINGER_INDEX = 1;

		[System.NonSerialized]
		private bool oldHeld;

#if UNITY_EDITOR
		protected virtual void Update()
		{
			var newHeld       = false;
			var enablePreview = false;
			var enablePaint   = false;

			//if (CwInput.GetMouseExists() == true)
			{
				CwInputManager.Finger finger;

				newHeld       = P3dSceneTool.LastSet;
				enablePaint   = newHeld == true || oldHeld == true;
				enablePreview = preview == true && enablePaint == false;

				if (enablePreview == true)
				{
					GetFinger(PREVIEW_FINGER_INDEX, P3dSceneTool.LastPosition, P3dSceneTool.LastPressure, true, out finger);

					cachedHitScreenBase.HandleFingerUpdate(finger, false, false);
				}

				if (enablePaint == true)
				{
					var down = GetFinger(PAINT_FINGER_INDEX, P3dSceneTool.LastPosition, P3dSceneTool.LastPressure, true, out finger);

					cachedHitScreenBase.HandleFingerUpdate(finger, down, newHeld == false);
				}
			}

			if (enablePreview == false)
			{
				TryNullFinger(PREVIEW_FINGER_INDEX);
			}

			if (enablePaint == false)
			{
				TryNullFinger(PAINT_FINGER_INDEX);
			}

			oldHeld = newHeld;
		}
#endif
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = P3dPointer_InEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class P3dPointer_InEditor_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("preview", "If you enable this, then a paint preview will be shown under the mouse as long as the <b>RequiredKey</b> is not pressed.");
		}
	}
}
#endif