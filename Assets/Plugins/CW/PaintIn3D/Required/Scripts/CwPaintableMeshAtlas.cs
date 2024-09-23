using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using UnityEngine.Serialization;

namespace PaintIn3D
{
	/// <summary>If you want to paint a mesh that is part of a texture atlas, except for the main mesh, you can add this component to all other GameObjects that are part of the texture atlas. You can then set the <b>Parent</b> setting to the main mesh.
	/// NOTE: This GameObject must have the <b>MeshFilter + MeshRenderer</b>, or <b>SkinnedMeshRenderer</b> component.
	/// NOTE: This GameObject should NOT have any <b>CwPaintableMeshTexture</b> components. Those should only be on the main (<b>Parent</b>) paintable mesh GameObject.</summary>
	[RequireComponent(typeof(Renderer))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMeshAtlas")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh Atlas")]
	public class CwPaintableMeshAtlas : CwMeshModel
	{
		/// <summary>The paintable mesh this atlas mesh is associated with.</summary>
		public virtual CwPaintableMesh Parent { set { parent = value; } get { return parent; } } [SerializeField] [FormerlySerializedAs("paintable")] protected CwPaintableMesh parent;

		public override bool IsActivated
		{
			get
			{
				if (parent != null && parent != this)
				{
					return parent.IsActivated;
				}

				return false;
			}
		}

		public override void Activate()
		{
			if (parent != null && parent != this)
			{
				parent.Activate();
			}
		}

		public override List<CwPaintableTexture> FindPaintableTextures(CwGroup group)
		{
			if (parent != null && parent != this)
			{
				return parent.FindPaintableTextures(group);
			}

			return null;
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwPaintableMeshAtlas;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPaintableMeshAtlas_Editor : CwMeshModel_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Parent == null));
				Draw("parent", "The paintable this separate paintable is associated with.");
			EndError();
			
			base.OnInspector();
		}
	}
}
#endif