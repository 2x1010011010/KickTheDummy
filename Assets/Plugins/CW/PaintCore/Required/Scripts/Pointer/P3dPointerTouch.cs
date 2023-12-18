using CW.Common;
using System.Collections.Generic;
using UnityEngine;

namespace PaintIn3D
{
	/// <summary>This component sends pointer information to any <b>P3dHitScreen</b> component, allowing you to paint with a touchscreen.</summary>
	[RequireComponent(typeof(P3dHitScreenBase))]
	[HelpURL(P3dCommon.HelpUrlPrefix + "P3dPointerTouch")]
	[AddComponentMenu(P3dCommon.ComponentHitMenuPrefix + "Pointer Touch")]
	public class P3dPointerTouch : P3dPointer
	{
		/// <summary>If you want the paint to appear above the finger, then you can set this number to something positive.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] private float offset;

		protected virtual void Update()
		{
			CwInputManager.Finger finger;

			for (var i = 0; i < CwInput.GetTouchCount(); i++)
			{
				int     index;
				Vector2 position;
				float   pressure;
				bool    set;

				CwInput.GetTouch(i, out index, out position, out pressure, out set);

				position.y += offset * CwInputManager.ScaleFactor;

				var down = GetFinger(index, position, pressure, set, out finger);

				cachedHitScreenBase.HandleFingerUpdate(finger, down, set == false);

				if (set == false)
				{
					TryNullFinger(index);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = P3dPointerTouch;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class P3dPointerTouch_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("offset");
		}
	}
}
#endif