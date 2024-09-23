using UnityEngine;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This component automatically destroys this GameObject after some time.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwDestroyAfterTime")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Destroy After Time")]
	public class CwDestroyAfterTime : MonoBehaviour
	{
		/// <summary>If this component has been active for this many seconds, the current GameObject will be destroyed.
		/// -1 = DestroyNow must be manually called.</summary>
		public float Seconds { set { seconds = value; } get { return seconds; } } [SerializeField] private float seconds = 5.0f;

		[SerializeField]
		private float age;

		[ContextMenu("Destroy Now")]
		public void DestroyNow()
		{
			Destroy(gameObject);
		}

		protected virtual void Update()
		{
			if (seconds >= 0.0f)
			{
				age += Time.deltaTime;

				if (age >= seconds)
				{
					DestroyNow();
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwDestroyAfterTime;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwDestroyAfterTime_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("seconds", "If this component has been active for this many seconds, the current GameObject will be destroyed.\n-1 = DestroyNow must be manually called.");
		}
	}
}
#endif