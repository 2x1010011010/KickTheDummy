using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This component marks the current GameObject as being paintable.
	/// NOTE: This GameObject must have the <b>MeshFilter + MeshRenderer</b>, or <b>SkinnedMeshRenderer</b> component.
	/// NOTE: If your mesh is part of a texture atlas, then you can use the <b>CwPaintableMeshAtlas</b> component on all the other atlas mesh GameObjects.</summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Renderer))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableMesh")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Mesh")]
	public class CwPaintableMesh : CwMeshModel
	{
		public enum ActivationType
		{
			Awake,
			OnEnable,
			Start,
			OnFirstUse
		}

		/// <summary>This allows you to control when this component actually activates and becomes ready for painting. You probably don't need to change this.</summary>
		public ActivationType Activation { set { activation = value; } get { return activation; } } [SerializeField] private ActivationType activation = ActivationType.Start;

		/// <summary>If this material is used in multiple renderers, you can specify them here. This usually happens with different LOD levels.</summary>
		public List<Renderer> OtherRenderers { set { otherRenderers = value; } get { return otherRenderers; } } [SerializeField] private List<Renderer> otherRenderers;

		/// <summary>This event will be invoked before this component is activated.</summary>
		public UnityEvent OnActivating { get { if (onActivating == null) onActivating = new UnityEvent(); return onActivating; } } [SerializeField] private UnityEvent onActivating;

		/// <summary>This event will be invoked after this component is activated.</summary>
		public UnityEvent OnActivated { get { if (onActivated == null) onActivated = new UnityEvent(); return onActivated; } } [SerializeField] private UnityEvent onActivated;

		/// <summary>This event will be invoked before this component is deactivated.</summary>
		public UnityEvent OnDeactivating { get { if (onDeactivating == null) onDeactivating = new UnityEvent(); return onDeactivating; } } [SerializeField] private UnityEvent onDeactivating;

		/// <summary>This event will be invoked after this component is deactivated.</summary>
		public UnityEvent OnDeactivated { get { if (onDeactivated == null) onDeactivated = new UnityEvent(); return onDeactivated; } } [SerializeField] private UnityEvent onDeactivated;

		[SerializeField]
		private bool activated;

		[System.NonSerialized]
		private HashSet<CwPaintableMeshTexture> paintableTextures = new HashSet<CwPaintableMeshTexture>();

		[System.NonSerialized]
		private static List<CwPaintableTexture> tempPaintableTextures = new List<CwPaintableTexture>();

		[System.NonSerialized]
		private static List<CwPaintableMeshTexture> tempPaintableMeshTextures = new List<CwPaintableMeshTexture>();

		/// <summary>This lets you know if this paintable has been activated.
		/// Being activated means each associated CwMaterialCloner and CwPaintableTexture has been Activated.
		/// NOTE: If you manually add CwMaterialCloner or CwPaintableTexture components after activation, then you must manually Activate().</summary>
		public override bool IsActivated
		{
			get
			{
				return activated;
			}
		}

		/// <summary>This gives you all CwPaintableTexture components that have been activated.</summary>
		public HashSet<CwPaintableMeshTexture> PaintableTextures
		{
			get
			{
				return paintableTextures;
			}
		}

		/// <summary>This method will remove all <b>CwPaintableMesh</b> and <b>CwPaintableTexture</b> components from this GameObject.</summary>
		public override void RemoveComponents()
		{
			var paintableTextures = GetComponents<CwPaintableMeshTexture>();

			for (var i = paintableTextures.Length - 1; i >= 0; i--)
			{
				var paintableTexture = paintableTextures[i];

				paintableTexture.Deactivate();

				CwHelper.Destroy(paintableTexture);
			}

			CwHelper.Destroy(this);
		}

#if UNITY_EDITOR
		[ContextMenu("Activate", true)]
		private bool ActivateValidate()
		{
			return Application.isPlaying == true && activated == false;
		}
#endif

		/// <summary>This allows you to manually activate all attached CwMaterialCloner and CwPaintableTexture components.</summary>
		[ContextMenu("Activate")]
		public override void Activate()
		{
			DoActivate();
		}

		protected virtual void DoActivate()
		{
			if (onActivating != null)
			{
				onActivating.Invoke();
			}

			// Activate textures
			AddPaintableTextures(transform);

			foreach (var paintableTexture in paintableTextures)
			{
				paintableTexture.Activate();
			}

			activated = true;

			if (onActivated != null)
			{
				onActivated.Invoke();
			}
		}

		private void AddPaintableTextures(Transform root)
		{
			root.GetComponents(tempPaintableMeshTextures);

			foreach (var paintableTexture in tempPaintableMeshTextures)
			{
				paintableTextures.Add(paintableTexture);
			}

			tempPaintableMeshTextures.Clear();

			for (var i = 0; i < root.childCount; i++)
			{
				var child = root.GetChild(i);

				if (child.GetComponent<CwPaintableMesh>() == null)
				{
					AddPaintableTextures(child);
				}
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Deactivate", true)]
		private bool DeactivateValidate()
		{
			return activated == true;
		}
#endif

		/// <summary>This reverses the material cloning.</summary>
		[ContextMenu("Deactivate")]
		public void Deactivate()
		{
			if (activated == true)
			{
				activated = false;

				DoDeactivate();
			}
		}

		protected virtual void DoDeactivate()
		{
			if (onDeactivating != null)
			{
				onDeactivating.Invoke();
			}

			foreach (var paintableTexture in paintableTextures)
			{
				if (paintableTexture != null)
				{
					paintableTexture.Deactivate();
				}
			}

			paintableTextures.Clear();

			if (onDeactivated != null)
			{
				onDeactivated.Invoke();
			}
		}

		/// <summary>This allows you to clear the pixels of all activated CwPaintableTexture components associated with this CwPaintable with the specified color.</summary>
		public void ClearAll(Color color)
		{
			ClearAll(default(Texture), color);
		}

		/// <summary>This allows you to clear the pixels of all activated CwPaintableTexture components associated with this CwPaintable with the specified color and texture.</summary>
		public void ClearAll(Texture texture, Color color)
		{
			if (activated == true)
			{
				foreach (var paintableTexture in paintableTextures)
				{
					paintableTexture.Clear(texture, color);
				}
			}
		}

		/// <summary>This allows you to manually register a CwPaintableTexture.</summary>
		public void Register(CwPaintableMeshTexture paintableTexture)
		{
			paintableTextures.Add(paintableTexture);
		}

		/// <summary>This allows you to manually unregister a CwPaintableTexture.</summary>
		public void Unregister(CwPaintableMeshTexture paintableTexture)
		{
			paintableTextures.Remove(paintableTexture);
		}

		public override List<CwPaintableTexture> FindPaintableTextures(CwGroup group)
		{
			tempPaintableTextures.Clear();

			foreach (var paintableTexture in paintableTextures)
			{
				if (paintableTexture.Group == group)
				{
					tempPaintableTextures.Add(paintableTexture);
				}
			}

			return tempPaintableTextures;
		}

		protected virtual void Awake()
		{
			if (activation == ActivationType.Awake && activated == false)
			{
				Activate();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (activation == ActivationType.OnEnable && activated == false)
			{
				Activate();
			}

			CwPaintableManager.GetOrCreateInstance();
		}

		protected virtual void Start()
		{
			if (activation == ActivationType.Start && activated == false)
			{
				Activate();
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwPaintableMesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPaintable_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (Any(tgts, t => t.IsActivated == true))
			{
				Info("This component has been activated.");
			}

			if (Any(tgts, t => t.IsActivated == true && Application.isPlaying == false))
			{
				Error("This component shouldn't be activated during edit mode. Deactivate it from the component context menu.");
			}

			Draw("activation", "This allows you to control when this component actually activates and becomes ready for painting. You probably don't need to change this.");

			Separator();

			if (Any(tgts, t => t.GetComponentInChildren<CwPaintableMeshTexture>() == null))
			{
				Warning("Your paintable doesn't have any paintable textures!");
			}

			if (Button("Add Paintable Mesh Texture") == true)
			{
				Each(tgts, t => t.gameObject.AddComponent<CwPaintableMeshTexture>());
			}

			if (Button("Analyze Mesh") == true)
			{
				CwMeshAnalysis.OpenWith(tgt.gameObject, tgt.PreparedMesh);
			}

			var mesh = PaintCore.CwCommon.GetMesh(tgt.gameObject, tgt.PreparedMesh);

			if (mesh != null && mesh.isReadable == false)
			{
				Error("You must set the Read/Write Enabled setting in this object's Mesh import settings.");
			}

			Separator();

			if (DrawFoldout("Advanced", "Show advanced settings?") == true)
			{
				BeginIndent();
					Draw("baseScale", "If you want the paintable texture width/height to be multiplied by the scale of this GameObject, this allows you to set the scale where you want the multiplier to be 1.");
					Draw("includeScale", "Transform the mesh with its position, rotation, and scale? Some skinned mesh setups require this to be disabled.");
					Draw("useMesh", "This allows you to choose how the Mesh attached to the current Renderer is used when painting.\n\nAsIs = Use what is currently set in the renderer.\n\nAutoSeamFix = Use (or automatically generate) a seam-fixed version of the mesh currently set in the renderer.");
					Draw("hash", "The hash code for this model used for de/serialization of this instance.");
					Draw("otherRenderers", "If this material is used in multiple renderers, you can specify them here. This usually happens with different LOD levels.");
					Draw("onActivating");
					Draw("onActivated");
					Draw("onDeactivating");
					Draw("onDeactivated");
				EndIndent();
			}
		}
	}
}
#endif