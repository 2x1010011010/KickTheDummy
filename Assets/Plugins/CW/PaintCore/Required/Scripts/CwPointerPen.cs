#if ENABLE_INPUT_SYSTEM && __INPUTSYSTEM__
	#define USE_NEW_INPUT_SYSTEM
#endif
using CW.Common;
using System.Collections.Generic;
using UnityEngine;

namespace PaintCore
{
	/// <summary>This component sends pointer information to any <b>CwHitScreen</b> component, allowing you to paint with a pen.</summary>
	[RequireComponent(typeof(CwHitScreenBase))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPointerPen")]
	[AddComponentMenu(CwCommon.ComponentHitMenuPrefix + "Pointer Pen")]
	public class CwPointerPen : CwPointer
	{
		/// <summary>If you enable this, then a paint preview will be shown under the pen as long as the tip is not pressed.</summary>
		public bool Preview { set { preview = value; } get { return preview; } } [SerializeField] private bool preview;

		/// <summary>If you want the paint to appear above the pen, then you can set this number to something positive.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] private float offset;

		private readonly int PREVIEW_FINGER_INDEX = -1;

		private readonly int PAINT_FINGER_INDEX = 1;

		[System.NonSerialized]
		private bool oldHeld;

		protected virtual void Update()
		{
			var newHeld       = false;
			var enablePreview = false;
			var enablePaint   = false;

			if (GetPenExists() == true)
			{
				CwInputManager.Finger finger;

				newHeld       = GetPenHeld();
				enablePaint   = newHeld == true || oldHeld == true;
				enablePreview = preview == true && enablePaint == false;

				if (enablePreview == true)
				{
					GetFinger(PREVIEW_FINGER_INDEX, GetPenPosition(), GetPenPressure(), true, out finger);

					cachedHitScreenBase.HandleFingerUpdate(finger, false, false);
				}

				if (enablePaint == true)
				{
					var down = GetFinger(PAINT_FINGER_INDEX, GetPenPosition(), GetPenPressure(), true, out finger);

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

		public static bool GetPenExists()
		{
#if USE_NEW_INPUT_SYSTEM
			return UnityEngine.InputSystem.Pen.current != null;
#endif
			return false;
		}

		public static Vector2 GetPenPosition()
		{
#if USE_NEW_INPUT_SYSTEM
			return UnityEngine.InputSystem.Pen.position.ReadValue();
#endif
			return Vector2.zero;
		}

		public static float GetPenPressure()
		{
#if USE_NEW_INPUT_SYSTEM
			return UnityEngine.InputSystem.Pen.pressure.ReadValue();
#endif
			return 0.0f;
		}

		public static bool GetPenHeld()
		{
#if USE_NEW_INPUT_SYSTEM
			return UnityEngine.InputSystem.Pen.tip.isPressed;
#endif
			return false;
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwPointerPen;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPointerPen_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("offset");
		}
	}
}
#endif