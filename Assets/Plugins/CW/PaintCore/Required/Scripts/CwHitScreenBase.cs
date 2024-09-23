using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This class contains common code for screen based mouse/finger hit components.</summary>
	public abstract class CwHitScreenBase : MonoBehaviour
	{
		public enum RotationType
		{
			Normal,
			World,
			ThisRotation,
			ThisLocalRotation,
			CustomRotation,
			CustomLocalRotation
		}

		public enum RelativeType
		{
			WorldUp,
			CameraUp,
			DrawAngle
		}

		public enum DirectionType
		{
			HitNormal,
			RayDirection,
			CameraDirection
		}

		public enum EmitType
		{
			PointsIn3D    = 0,
			PointsOnUV    = 20,
			TrianglesIn3D = 30
		}

		[System.Flags]
		public enum ButtonTypes
		{
			LeftMouse   = 1 << 0,
			RightMouse  = 1 << 1,
			MiddleMouse = 1 << 2,
			Touch       = 1 << 5
		}

		/// <summary>Orient to a specific camera?
		/// None = MainCamera.</summary>
		public Camera Camera { set { _camera = value; } get { return _camera; } } [SerializeField] private Camera _camera;

		/// <summary>The layers you want the raycast to hit.</summary>
		public LayerMask Layers { set { layers = value; } get { return layers; } } [SerializeField] private LayerMask layers = Physics.DefaultRaycastLayers;

		/// <summary>Fingers that began touching the screen on top of these UI layers will be ignored.</summary>
		public LayerMask GuiLayers { set { guiLayers = value; } get { return guiLayers; } } [SerializeField] private LayerMask guiLayers = 1 << 5;

		/// <summary>This allows you to control the hit data this component sends out.
		/// PointsIn3D = Point drawing in 3D.
		/// PointsOnUV = Point drawing on UV (requires non-convex <b>MeshCollider</b>).
		/// TrianglesIn3D = Triangle drawing in 3D (requires non-convex <b>MeshCollider</b>).</summary>
		public EmitType Emit { set { emit = value; } get { return emit; } } [UnityEngine.Serialization.FormerlySerializedAs("draw")] [SerializeField] private EmitType emit;

		/// <summary>This allows you to control how the paint is rotated.
		/// Normal = The rotation will be based on a normal direction, and rolled relative to an up axis.
		/// World = The rotation will be aligned to the world, or given no rotation.
		/// ThisRotation = The current <b>Transform.rotation</b> will be used.
		/// ThisLocalRotation = The current <b>Transform.localRotation</b> will be used.
		/// CustomRotation = The specified <b>CustomTransform.rotation</b> will be used.
		/// CustomLocalRotation = The specified <b>CustomTransform.localRotation</b> will be used.</summary>
		public RotationType RotateTo { set { rotateTo = value; } get { return rotateTo; } } [SerializeField] private RotationType rotateTo;

		/// <summary>Which direction should the hit point rotation be based on?</summary>
		public DirectionType NormalDirection { set { normalDirection = value; } get { return normalDirection; } } [UnityEngine.Serialization.FormerlySerializedAs("normal")] [SerializeField] private DirectionType normalDirection = DirectionType.CameraDirection;

		/// <summary>Based on the normal direction, what should the rotation be rolled relative to?
		/// WorldUp = It will be rolled so the up vector is world up.
		/// CameraUp = It will be rolled so the up vector is camera up.
		/// DrawAngle = It will be rolled according to the mouse/finger movement on screen.</summary>
		public RelativeType NormalRelativeTo { set { normalRelativeTo = value; } get { return normalRelativeTo; } } [UnityEngine.Serialization.FormerlySerializedAs("orientation")] [SerializeField] private RelativeType normalRelativeTo = RelativeType.CameraUp;

		/// <summary>This allows you to specify the <b>Transform</b> when using <b>RotateTo = CustomRotation/CustomLocalRotation</b>.</summary>
		public Transform CustomTransform { set { customTransform = value; } get { return customTransform; } } [SerializeField] private Transform customTransform;

		/// <summary>Should painting triggered from this component be eligible for being undone?</summary>
		public bool StoreStates { set { storeStates = value; } get { return storeStates; } } [SerializeField] protected bool storeStates = true;

		/// <summary>This allows you to override the order this paint gets applied to the object during the current frame.</summary>
		public int Priority { set { priority = value; } get { return priority; } } [SerializeField] private int priority;

		// No longer used
		[SerializeField] private ButtonTypes requiredButtons;

		// No longer used
		[SerializeField] private KeyCode requiredKey;

		// No longer used
		[SerializeField] private float normalOffset;

		// No longer used
		[SerializeField] private float mouseOffset;

		// No longer used
		[SerializeField] private float touchOffset;

		// No longer used
		[SerializeField] private bool showPreview;

		public bool NeedsDrawAngle
		{
			get
			{
				return rotateTo == RotationType.Normal && normalRelativeTo == RelativeType.DrawAngle;
			}
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (GetComponent<CwPointerMouse>() == null)
			{
				requiredButtons &= ~ButtonTypes.LeftMouse;
				requiredButtons &= ~ButtonTypes.RightMouse;
				requiredButtons &= ~ButtonTypes.MiddleMouse;

				gameObject.AddComponent<CwPointerMouse>();
			}

			if (GetComponent<CwPointerTouch>() == null)
			{
				requiredButtons &= ~ButtonTypes.Touch;

				gameObject.AddComponent<CwPointerTouch>();
			}

			if (GetComponent<CwPointerPen>() == null)
			{
				gameObject.AddComponent<CwPointerPen>();
			}
		}
#endif

		public bool ShouldUpgradePointers()
		{
			return (requiredButtons & ButtonTypes.LeftMouse  ) != 0
				|| (requiredButtons & ButtonTypes.RightMouse ) != 0
				|| (requiredButtons & ButtonTypes.MiddleMouse) != 0
				|| (requiredButtons & ButtonTypes.Touch      ) != 0;
		}

		public void TryUpgradePointers()
		{
			var lmb = (requiredButtons & ButtonTypes.LeftMouse  ) != 0;
			var rmb = (requiredButtons & ButtonTypes.RightMouse ) != 0;
			var mmb = (requiredButtons & ButtonTypes.MiddleMouse) != 0;

			if (lmb == true || rmb == true || mmb == true)
			{
				requiredButtons &= ~ButtonTypes.LeftMouse;
				requiredButtons &= ~ButtonTypes.RightMouse;
				requiredButtons &= ~ButtonTypes.MiddleMouse;

				var pointerMouse = gameObject.AddComponent<CwPointerMouse>();

				pointerMouse.Preview = showPreview;

				if (lmb == true)
				{
					pointerMouse.TryAddKey(KeyCode.Mouse0);
				}

				if (rmb == true)
				{
					pointerMouse.TryAddKey(KeyCode.Mouse1);
				}

				if (mmb == true)
				{
					pointerMouse.TryAddKey(KeyCode.Mouse2);
				}
			}

			if ((requiredButtons & ButtonTypes.Touch) != 0)
			{
				requiredButtons &= ~ButtonTypes.Touch;

				var pointerTouch = gameObject.AddComponent<CwPointerTouch>();

				pointerTouch.Offset = touchOffset;
			}
		}

		public virtual void BreakFinger(CwInputManager.Finger finger)
		{
		}

		public virtual void HandleFingerUpdate(CwInputManager.Finger finger, bool down, bool up)
		{
			if (up == true)
			{
				HandleFingerUp(finger);
			}
		}

		protected virtual void HandleFingerUp(CwInputManager.Finger finger)
		{
		}

		protected virtual void DoQuery(Vector2 screenPosition, ref Camera camera, ref Ray ray, ref CwHit hit3D, ref RaycastHit2D hit2D)
		{
			var hit = default(RaycastHit);

			camera = CwHelper.GetCamera(_camera);
			ray    = camera.ScreenPointToRay(screenPosition);
			hit2D  = Physics2D.GetRayIntersection(ray, float.PositiveInfinity, layers);

			Physics.Raycast(ray, out hit, float.PositiveInfinity, layers);

			hit3D = new CwHit(hit);
		}

		protected void PaintAt(CwPointConnector connector, CwHitCache hitCache, Vector2 screenPosition, Vector2 screenPositionOld, bool preview, float pressure, object owner)
		{
			var offset = CwInput.GetTouchCount() > 0 ? touchOffset : mouseOffset;

			if (offset != 0.0f)
			{
				screenPosition.y    += offset / CwInputManager.ScaleFactor;
				screenPositionOld.y += offset / CwInputManager.ScaleFactor;
			}

			var camera        = default(Camera);
			var ray           = default(Ray);
			var hit2D         = default(RaycastHit2D);
			var hit3D         = default(CwHit);
			var finalPosition = default(Vector3);
			var finalRotation = default(Quaternion);

			DoQuery(screenPosition, ref camera, ref ray, ref hit3D, ref hit2D);

			var valid2D = hit2D.distance > 0.0f;
			var valid3D = hit3D.Distance > 0.0f;

			// Hit 3D?
			if (valid3D == true && (valid2D == false || hit3D.Distance < hit2D.distance))
			{
				CalcHitData(hit3D.Position, hit3D.Normal, ray, camera, screenPositionOld, ref finalPosition, ref finalRotation);

				if (emit == EmitType.PointsIn3D)
				{
					if (connector != null)
					{
						connector.SubmitPoint(gameObject, preview, priority, pressure, finalPosition, finalRotation, owner);
					}
					else
					{
						hitCache.InvokePoint(gameObject, preview, priority, pressure, finalPosition, finalRotation);
					}

					return;
				}
				else if (emit == EmitType.PointsOnUV)
				{
					hitCache.InvokeCoord(gameObject, preview, priority, pressure, hit3D, finalRotation);

					return;
				}
				else if (emit == EmitType.TrianglesIn3D)
				{
					hitCache.InvokeTriangle(gameObject, preview, priority, pressure, hit3D, finalRotation);

					return;
				}
			}
			// Hit 2D?
			else if (valid2D == true)
			{
				CalcHitData(hit2D.point, new Vector3(0.0f, 0.0f, -1.0f), ray, camera, screenPositionOld, ref finalPosition, ref finalRotation);

				if (emit == EmitType.PointsIn3D)
				{
					if (connector != null)
					{
						connector.SubmitPoint(gameObject, preview, priority, pressure, finalPosition, finalRotation, owner);
					}
					else
					{
						hitCache.InvokePoint(gameObject, preview, priority, pressure, finalPosition, finalRotation);
					}

					return;
				}
			}

			if (connector != null)
			{
				connector.BreakHits(owner);
			}
		}

		private void CalcHitData(Vector3 hitPoint, Vector3 hitNormal, Ray ray, Camera camera, Vector2 screenPositionOld, ref Vector3 finalPosition, ref Quaternion finalRotation)
		{
			finalPosition = hitPoint + hitNormal * normalOffset;
			finalRotation = Quaternion.identity;

			switch (rotateTo)
			{
				case RotationType.Normal:
				{
					var finalNormal = default(Vector3);

					switch (normalDirection)
					{
						case DirectionType.HitNormal: finalNormal = hitNormal; break;
						case DirectionType.RayDirection: finalNormal = -ray.direction; break;
						case DirectionType.CameraDirection: finalNormal = -camera.transform.forward; break;
					}

					var finalUp = Vector3.up;

					switch (normalRelativeTo)
					{
						case RelativeType.CameraUp: finalUp = camera.transform.up; break;
						case RelativeType.DrawAngle:
						{
							var rayOld = camera.ScreenPointToRay(screenPositionOld);

							if (camera.orthographic == true)
							{
								finalUp = Vector3.Cross(rayOld.GetPoint(1.0f) - ray.origin, ray.GetPoint(1.0f) - ray.origin);
							}
							else
							{
								finalUp = Vector3.Cross(rayOld.direction, ray.direction);
							}
						}

						break;
					}

					finalRotation = Quaternion.LookRotation(-finalNormal, finalUp);
				}
				break;
				case RotationType.World: finalRotation = Quaternion.identity; break;
				case RotationType.ThisRotation: finalRotation = transform.rotation; break;
				case RotationType.ThisLocalRotation: finalRotation = transform.localRotation; break;
				case RotationType.CustomRotation: if (customTransform != null) finalRotation = customTransform.rotation; break;
				case RotationType.CustomLocalRotation: if (customTransform != null) finalRotation = customTransform.localRotation; break;
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwHitScreenBase;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwHitScreenBase_Editor : CwEditor
	{
		protected virtual void DrawBasic()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (Any(tgts, t => t.ShouldUpgradePointers() == true))
			{
				if (HelpButton("This component is using legacy control settings and won't be able to paint anything.", MessageType.Warning, "Upgrade", 80) == true)
				{
					Each(tgts, t => t.TryUpgradePointers(), true, true);
				}
			}

			Draw("emit", "This allows you to control the hit data this component sends out.\n\nPointsIn3D = Point drawing in 3D.\n\nPointsOnUV = Point drawing on UV (requires non-convex MeshCollider).\n\nTrianglesIn3D = Triangle drawing in 3D (requires non-convex MeshCollider).");
			BeginError(Any(tgts, t => t.Layers == 0));
				Draw("layers", "The layers you want the raycast to hit.");
			EndError();
			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");
			BeginError(Any(tgts, t => CwHelper.GetCamera(t.Camera) == null));
				Draw("_camera", "Orient to a specific camera?\n\nNone = MainCamera.");
			EndError();

			Separator();

			Draw("rotateTo", "This allows you to control how the paint is rotated.\n\nNormal = The rotation will be based on a normal direction, and rolled relative to an up axis.\n\nWorld = The rotation will be aligned to the world, or given no rotation.\n\nThisRotation = The current Transform.rotation will be used.\n\nThisLocalRotation = The current Transform.localRotation will be used.\n\nCustomRotation = The specified Transform.rotation will be used.\n\nCustomLocalRotation = The specified Transform.localRotation will be used.");
			if (Any(tgts, t => t.RotateTo == CwHitScreenBase.RotationType.Normal))
			{
				BeginIndent();
					Draw("normalDirection", "Which direction should the hit point rotation be based on?", "Direction");
					Draw("normalRelativeTo", "Based on the normal direction, what should the rotation be rolled relative to?\n\nWorldUp = It will be rolled so the up vector is world up.\n\nCameraUp = It will be rolled so the up vector is camera up.\n\nDrawAngle = It will be rolled according to the mouse/finger movement on screen.", "Relative To");
				EndIndent();
			}
			if (Any(tgts, t => t.RotateTo == CwHitScreenBase.RotationType.CustomRotation || t.RotateTo == CwHitScreenBase.RotationType.CustomLocalRotation))
			{
				BeginIndent();
					Draw("customTransform", "This allows you to specify the Transform when using RotateTo = CustomRotation/CustomLocalRotation.");
				EndIndent();
			}
		}

		protected void DrawAdvancedFoldout()
		{
			if (DrawFoldout("Advanced", "Show advanced settings?") == true)
			{
				BeginIndent();
					DrawAdvanced();
				EndIndent();
			}
		}

		protected virtual void DrawAdvanced()
		{
			Draw("storeStates", "Should painting triggered from this component be eligible for being undone?");
			Draw("priority", "This allows you to override the order this paint gets applied to the object during the current frame.");
			Draw("normalOffset", "If you want the raycast hit point to be offset from the surface a bit, this allows you to set by how much in world space.");
		}
	}
}
#endif