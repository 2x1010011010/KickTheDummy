using CW.Common;
using System.Collections.Generic;
using UnityEngine;
using static CW.Common.CwInputManager;

namespace PaintCore
{
	/// <summary>This this is the base class for any component that sends pointer information to any <b>CwHitScreen</b> component.</summary>
	[RequireComponent(typeof(CwHitScreenBase))]
	public abstract class CwPointer : MonoBehaviour
	{
		public class VirtualFinger
		{
			public Vector2 Position;
		}

		[System.NonSerialized]
		protected CwHitScreenBase cachedHitScreenBase;

		[System.NonSerialized]
		private List<CwInputManager.Finger> fingers = new List<CwInputManager.Finger>();

		public bool GetFinger(int index, Vector2 position, float pressure, bool set, out CwInputManager.Finger finger)
		{
			for (var i = 0; i < fingers.Count; i++)
			{
				finger = fingers[i];

				if (finger.Index == index)
				{
					StepFinger(finger, position, pressure, set);

					return false;
				}
			}

			finger = new CwInputManager.Finger();

			fingers.Add(finger);

			InitFinger(finger, index, position, pressure, set, cachedHitScreenBase.GuiLayers);

			return true;
		}

		public bool TryNullFinger(int index)
		{
			for (var i = 0; i < fingers.Count; i++)
			{
				var finger = fingers[i];

				if (finger.Index == index)
				{
					cachedHitScreenBase.BreakFinger(finger);

					fingers.RemoveAt(i);

					return true;
				}
			}

			return false;
		}

		protected virtual void OnEnable()
		{
			cachedHitScreenBase = GetComponent<CwHitScreenBase>();
		}

		private void InitFinger(Finger finger, int index, Vector2 screenPosition, float pressure, bool set, int guiLayers)
		{
			finger.Index = index;
			finger.Down  = true;
			finger.Age   = 0.0f;

			finger.StartedOverGui          = PointOverGui(screenPosition, guiLayers);
			finger.StartScreenPosition     = screenPosition;
			finger.ScreenPositionOld       = screenPosition;
			finger.ScreenPositionOldOld    = screenPosition;
			finger.ScreenPositionOldOldOld = screenPosition;

			finger.Pressure       = pressure;
			finger.ScreenPosition = screenPosition;
			finger.Up             = set == false;
		}

		private void StepFinger(Finger finger, Vector2 screenPosition, float pressure, bool set)
		{
			finger.Down = false;
			finger.Age += Time.deltaTime;

			finger.ScreenPositionOldOldOld = finger.ScreenPositionOldOld;
			finger.ScreenPositionOldOld    = finger.ScreenPositionOld;
			finger.ScreenPositionOld       = finger.ScreenPosition;

			finger.Pressure       = pressure;
			finger.ScreenPosition = screenPosition;
			finger.Up             = set == false;
		}
	}
}