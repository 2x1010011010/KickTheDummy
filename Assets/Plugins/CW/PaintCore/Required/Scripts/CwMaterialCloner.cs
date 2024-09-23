using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace PaintCore
{
	/// <summary>This component allows you to duplicate a material before you paint on it. This is useful if the material is shared between multiple GameObjects (e.g. prefabs).</summary>
	[DefaultExecutionOrder(-100)]
	[RequireComponent(typeof(CwModel))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwMaterialCloner")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Material Cloner")]
	public class CwMaterialCloner : MonoBehaviour
	{
		public struct External
		{
			public Renderer Root;
			public int      Index;
		}

		/// <summary>The material index that will be cloned. This matches the Materials list in your MeshRenderer/SkinnedMeshRenderer, where 0 is the first material.</summary>
		public int Index { set { index = value; } get { return index; } } [SerializeField] private int index;

		/// <summary>If this shader needs specific keywords enabled, you can add them here.</summary>
		public string ShaderKeyword { set { shaderKeyword = value; } get { return shaderKeyword; } } [SerializeField] private string shaderKeyword;

		/// <summary>The cloned material also belongs to these external GameObjects. This is used with texture atlasing.</summary>
		public List<External> Externals { set { externals = value; } get { return externals; } } [SerializeField] private List<External> externals;

		[SerializeField]
		private bool activated;

		[SerializeField]
		private Material current;

		[SerializeField]
		private Material original;

		[System.NonSerialized]
		private static List<Material> tempMaterials = new List<Material>();

		public Material Original
		{
			get
			{
				return original;
			}
		}

		public Material Current
		{
			get
			{
				return current;
			}
		}

		/// <summary>This lets you know if this component has already been activated and has executed.</summary>
		public bool Activated
		{
			get
			{
				return activated;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Activate", true)]
		private bool ActivateValidate()
		{
			return Application.isPlaying == true && activated == false;
		}
#endif

		/// <summary>This allows you to manually activate this component, cloning the specified material.
		/// NOTE: This will automatically be called from CwPaintable to clone the material.</summary>
		[ContextMenu("Activate")]
		public void Activate()
		{
			if (activated == false && index >= 0)
			{
				var renderer = GetComponent<Renderer>();

				renderer.GetSharedMaterials(tempMaterials);

				if (index >= 0 && index < tempMaterials.Count)
				{
					original = tempMaterials[index];

					if (original != null)
					{
						activated = true;
						current   = Instantiate(original);

						if (string.IsNullOrEmpty(shaderKeyword) == false)
						{
							current.EnableKeyword(shaderKeyword);
						}

						Replace(renderer, index, original, current);
					}
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

				Replace(GetComponent<Renderer>(), index, current, original);

				foreach (var external in externals)
				{
					Replace(external.Root, index, current, original);
				}

				current = CwHelper.Destroy(current);
			}
		}

		private void Replace(Renderer renderer, int index, Material oldMaterial, Material newMaterial)
		{
			renderer.GetSharedMaterials(tempMaterials);

			if (index >= 0 && index < tempMaterials.Count)
			{
				if (tempMaterials[index] == oldMaterial)
				{
					tempMaterials[index] = newMaterial;

					renderer.sharedMaterials = tempMaterials.ToArray();
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwMaterialCloner;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwMaterialCloner_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (Any(tgts, t => t.Activated == true))
			{
				Info("This component has been activated.");
			}

			if (Any(tgts, t => t.Activated == true && Application.isPlaying == false))
			{
				Error("This component shouldn't be activated during edit mode. Deactivate it from the component context menu.");
			}

			BeginError(Any(tgts, t => t.Index < 0 || t.Index >= t.GetComponent<Renderer>().sharedMaterials.Length));
				var showMaterial = DrawExpand("index", "The material index that will be cloned. This matches the Materials list in your MeshRenderer/SkinnedMeshRenderer, where 0 is the first material.");
			EndError();
			if (showMaterial == true)
			{
				BeginIndent();
					BeginDisabled();
						EditorGUILayout.ObjectField(new GUIContent("Material", "This is the current material at the specified material index."), CwCommon.GetMaterial(tgt.GetComponent<Renderer>(), tgt.Index), typeof(Material), false);
					EndDisabled();
				EndIndent();
			}
		}
	}
}
#endif