using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This component can generate a mask texture from the specified mesh. This can be used with the <b>CwPaintableTexture</b> component's <b>Advanced / LocalMask</b> setting.</summary>
	//[ExecuteInEditMode]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwGenerateMask")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Generate Mask")]
	public class CwGenerateMask : MonoBehaviour
	{
		public enum ApplyType
		{
			Manually,
			Siblings,
			SiblingsAndDescendants
		}

		[System.Serializable]
		public class RenderTextureEvent : UnityEvent<RenderTexture> {}

		/// <summary>The mask will be generated from this mesh.</summary>
		public Mesh Mesh { set { mesh = value; } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>The mask will be generated from this submesh of the mesh.</summary>
		public int Submesh { set { submesh = value; } get { return submesh; } } [SerializeField] private int submesh;

		/// <summary>The texture channel of the mesh the mask will be generated from.</summary>
		public CwCoord Coord { set { coord = value; } get { return coord; } } [SerializeField] private CwCoord coord;

		/// <summary>The size of the generated texture.</summary>
		public Vector2Int Size { set { size = value; } get { return size; } } [SerializeField] private Vector2Int size = new Vector2Int(512, 512);

		/// <summary>The format of the generated texture.</summary>
		public RenderTextureFormat Format { set { format = value; } get { return format; } } [SerializeField] private RenderTextureFormat format = RenderTextureFormat.R8;

		/// <summary>Should the generated mask be automatically applied?
		/// Manually = Use the <b>OnGenerated</b> event to apply the mask to specific components.
		/// Siblings = The mask will be applied to all <b>CwPaintableTexture</b> components on this GameObject.
		/// SiblingsAndDescendants = Like <b>Siblings</b>, but also all child GameObjects.</summary>
		public ApplyType ApplyTo { set { applyTo = value; } get { return applyTo; } } [SerializeField] private ApplyType applyTo = ApplyType.Siblings;

		/// <summary>After the mask is generated, this event will be invoked.</summary>
		public RenderTextureEvent OnGenerated { get { if (onGenerated == null) onGenerated = new RenderTextureEvent(); return onGenerated; } } [SerializeField] private RenderTextureEvent onGenerated;

		[System.NonSerialized]
		private RenderTexture generatedTexture;

		private static List<CwPaintableMeshTexture> tempPaintableTextures = new List<CwPaintableMeshTexture>();

		/// <summary>This allows you to access the generated texture.</summary>
		public RenderTexture GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

		/// <summary>This method will destroy the generated texture.</summary>
		[ContextMenu("Clear")]
		public void Clear()
		{
			DestroyImmediate(generatedTexture);

			generatedTexture = null;
		}

		/// <summary>This method will generate a mask texture based on the specified settings.</summary>
		[ContextMenu("Generate")]
		public RenderTexture Generate()
		{
			TryGenerate();

			return generatedTexture;
		}

		public bool TryGenerate()
		{
			Clear();

			if (size.x > 0 && size.y > 0)
			{
				generatedTexture = new RenderTexture(size.x, size.y, 0, format);

				generatedTexture.name = "Generated Mask";

				CwCommandReplace.Blit(generatedTexture, default(Texture), Color.black);

				CwBlit.White(generatedTexture, mesh, submesh, coord);

				if (applyTo != ApplyType.Manually)
				{
					if (applyTo == ApplyType.SiblingsAndDescendants)
					{
						GetComponentsInChildren(tempPaintableTextures);
					}
					else
					{
						GetComponents(tempPaintableTextures);
					}

					foreach (var tempPaintableTexture in tempPaintableTextures)
					{
						tempPaintableTexture.LocalMaskTexture = generatedTexture;
					}
				}

				if (onGenerated != null)
				{
					onGenerated.Invoke(generatedTexture);
				}

				return true;
			}

			return false;
		}

		protected virtual void OnEnable()
		{
			Generate();
		}

		protected virtual void OnDisable()
		{
			Clear();
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwGenerateMask;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwGenerateMask_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "The mask will be generated from this mesh.");
			EndError();
			Draw("submesh", "The mask will be generated from this submesh of the mesh.");
			Draw("coord", "The texture channel of the mesh the mask will be generated from.");
			Draw("size", "The size of the generated texture.");
			Draw("format", "The format of the generated texture.");
			Draw("applyTo", "Should the generated mask be automatically applied?\n\nManually = Use the <b>OnGenerated</b> event to apply the mask to specific components.\n\nSiblings = The mask will be applied to all <b>CwPaintableTexture</b> components on this GameObject.\n\nSiblingsAndDescendants = Like <b>Siblings</b>, but also all child GameObjects.");
			BeginDisabled();
				EditorGUI.ObjectField(Reserve(18), new GUIContent("Generated Texture", "This allows you to access the generated texture."), tgt.GeneratedTexture, typeof(Texture), false);
			EndDisabled();

			Separator();

			Draw("onGenerated");
		}
	}
}
#endif