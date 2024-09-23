using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to block paint from being applied at the current position using the specified shape.</summary>
	[ExecuteInEditMode]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwMask")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Mask")]
	public class CwMask : MonoBehaviour
	{
		/// <summary>The mask will use this texture shape.</summary>
		public Texture Texture { set { texture = value; } get { return texture; } } [SerializeField] private Texture texture;

		/// <summary>The mask will use pixels from this texture channel.</summary>
		public CwChannel Channel { set { channel = value; } get { return channel; } } [SerializeField] private CwChannel channel = CwChannel.Alpha;

		/// <summary>By default, opaque/white parts of the mask are areas you can paint, and transparent/black parts are parts that are masked. Invert this?</summary>
		public bool Invert { set { invert = value; } get { return invert; } } [SerializeField] private bool invert;

		/// <summary>If you want the sides of the mask to extend farther out, then this allows you to set the scale of the boundary.
		/// 1 = Default.
		/// 2 = Double size.</summary>
		public Vector2 Stretch { set { stretch = value; } get { return stretch; } } [SerializeField] private Vector2 stretch = Vector2.one;

		/// <summary>This stores all active and enabled instances in the open scenes.</summary>
		public static LinkedList<CwMask> Instances { get { return instances; } } private static LinkedList<CwMask> instances = new LinkedList<CwMask>(); private LinkedListNode<CwMask> instancesNode;

		public virtual Matrix4x4 Matrix
		{
			get
			{
				return transform.worldToLocalMatrix;
			}
		}

		public static CwMask Find(Vector3 position, LayerMask layers)
		{
			var bestMask     = default(CwMask);
			var bestDistance = float.PositiveInfinity;

			foreach (var instance in instances)
			{
				if (CwHelper.IndexInMask(instance.gameObject.layer, layers) == true)
				{
					var distance = Vector3.SqrMagnitude(position - instance.transform.position);

					if (distance < bestDistance)
					{
						bestDistance = distance;
						bestMask     = instance;
					}
				}
			}

			return bestMask;
		}

		protected virtual void OnEnable()
		{
			instancesNode = instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			instances.Remove(instancesNode); instancesNode = null;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var m = Matrix.inverse;

			Gizmos.matrix = m;

			Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 1.0f, 0.0f));
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 1.0f, 1.0f));
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(stretch.x, stretch.y, 0.0f));
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(stretch.x, stretch.y, 1.0f));

			for (var i = 0; i < 16; i++)
			{
				var subMatrix = m * Matrix4x4.Translate(new Vector3(0.0f, 0.0f, Mathf.Lerp(-0.5f, 0.5f, i / 15.0f)));

				CwCommon.DrawShapeOutline(texture, channel, subMatrix);
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwMask;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwMask_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Texture == null));
				Draw("texture", "The mask will use this texture shape.");
			EndError();
			Draw("channel", "The mask will use pixels from this texture channel.");
			Draw("invert", "By default, opaque/white parts of the mask are areas you can paint, and transparent/black parts are parts that are masked. Invert this?");
			Draw("stretch", "If you want the sides of the mask to extend farther out, then this allows you to set the scale of the boundary.\n\n1 = Default.\n\n2 = Double size.");
		}
	}
}
#endif