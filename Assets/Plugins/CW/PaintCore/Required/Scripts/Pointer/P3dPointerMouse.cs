using CW.Common;
using System.Collections.Generic;
using UnityEngine;
using static CW.Common.CwInputManager;

namespace PaintIn3D
{
	/// <summary>This component sends pointer information to any <b>P3dHitScreen</b> component, allowing you to paint with the mouse.</summary>
	[RequireComponent(typeof(P3dHitScreenBase))]
	[HelpURL(P3dCommon.HelpUrlPrefix + "P3dPointerMouse")]
	[AddComponentMenu(P3dCommon.ComponentHitMenuPrefix + "Pointer Mouse")]
	public class P3dPointerMouse : P3dPointer
	{
		/// <summary>If you enable this, then a paint preview will be shown under the mouse as long as the <b>RequiredKey</b> is not pressed.</summary>
		public bool Preview { set { preview = value; } get { return preview; } } [SerializeField] private bool preview;

		/// <summary>This component will paint while any of the specified mouse buttons or keyboard keys are held.</summary>
		public List<KeyCode> Keys { get { if (keys == null) { keys = new List<KeyCode>(); } return keys; } } [SerializeField] private List<KeyCode> keys;

		private readonly int PREVIEW_FINGER_INDEX = -1;

		private readonly int PAINT_FINGER_INDEX = 1;

		[System.NonSerialized]
		private bool oldHeld;

		public bool TryAddKey(KeyCode key)
		{
			if (Keys.Contains(key) == false)
			{
				keys.Add(key);

				return true;
			}

			return false;
		}

		public bool GetKeyHeld()
		{
			if (keys != null)
			{
				foreach (var key in keys)
				{
					if (CwInput.GetKeyIsHeld(key) == true)
					{
						return true;
					}
				}
			}

			return false;
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (keys == null)
			{
				keys = new List<KeyCode>();
			}
			else
			{
				keys.Clear();
			}

			keys.Add(KeyCode.Mouse0);
		}
#endif

		protected virtual void Update()
		{
			var newHeld       = false;
			var enablePreview = false;
			var enablePaint   = false;

			if (CwInput.GetMouseExists() == true)
			{
				CwInputManager.Finger finger;

				newHeld       = GetKeyHeld();
				enablePaint   = newHeld == true || oldHeld == true;
				enablePreview = preview == true && enablePaint == false;

				if (enablePreview == true)
				{
					GetFinger(PREVIEW_FINGER_INDEX, CwInput.GetMousePosition(), 1.0f, true, out finger);

					cachedHitScreenBase.HandleFingerUpdate(finger, false, false);
				}

				if (enablePaint == true)
				{
					var down = GetFinger(PAINT_FINGER_INDEX, CwInput.GetMousePosition(), 1.0f, true, out finger);

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
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = P3dPointerMouse;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class P3dPointerMouse_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("preview", "If you enable this, then a paint preview will be shown under the mouse as long as the <b>RequiredKey</b> is not pressed.");
			Draw("keys", "This component will paint while any of the specified mouse buttons or keyboard keys are held.");
		}
	}
}
#endif