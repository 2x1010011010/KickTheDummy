using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component grabs paint hits and connected hits, mirrors the data, then re-broadcasts it.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwCloneMirror")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Clone Mirror")]
	public class CwCloneMirror : CwClone
	{
		/// <summary>When a decal is mirrored it will appear backwards, should it be flipped back around?</summary>
		public bool Flip { set { flip = value; } get { return flip; } } [SerializeField] private bool flip;

		public override void Transform(ref Matrix4x4 posMatrix, ref Matrix4x4 rotMatrix, ref Matrix4x4 rotMatrix2)
		{
			var tr = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			var tr_i = tr.inverse;
			var r = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
			var r_i = r.inverse;
			var z = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f));

			posMatrix = tr * z * tr_i * posMatrix;
			rotMatrix = r * z * r_i * rotMatrix;

			if (flip == true)
			{
				rotMatrix2.m00 *= -1.0f;
				rotMatrix2.m10 *= -1.0f;
				rotMatrix2.m20 *= -1.0f;
				rotMatrix2.m30 *= -1.0f;
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			for (var i = 1; i <= 10; i++)
			{
				Gizmos.DrawWireCube(Vector3.zero, new Vector3(i, i, 0.0f));
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwCloneMirror;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwCloneMirror_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("flip", "When a decal is mirrored it will appear backwards, should it be flipped back around?");
		}
	}
}
#endif