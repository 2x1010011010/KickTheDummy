using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component marks the current GameObject as being paintable, as long as this GameObject has a MeshFilter + MeshRenderer, or a SkinnedMeshRenderer.
	/// NOTE: To actually paint, the <b>CwPaintableTexture</b> component must be on a different object.</summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Renderer))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwAtlasMesh")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Atlas Mesh")]
	public abstract class CwModel : MonoBehaviour
	{
		/// <summary>The hash code for this model used for de/serialization of this instance.</summary>
		public CwHash Hash { set { hash = value; CwSerialization.TryRegister(this, hash); } get { return hash; } } [SerializeField] protected CwHash hash;

		/// <summary>If you want the paintable texture width/height to be multiplied by the scale of this GameObject, this allows you to set the scale where you want the multiplier to be 1.</summary>
		public Vector3 BaseScale { set { baseScale = value; } get { return baseScale; } } [SerializeField] private Vector3 baseScale;

		[System.NonSerialized]
		protected Renderer cachedRenderer;

		[System.NonSerialized]
		protected bool cachedRendererSet;

		[System.NonSerialized]
		protected Transform cachedTransform;

		[System.NonSerialized]
		protected GameObject cachedGameObject;

		[System.NonSerialized]
		protected bool prepared;

		[System.NonSerialized]
		private static List<Material> tempMaterials = new List<Material>();

		[System.NonSerialized]
		private static List<CwModel> tempModels = new List<CwModel>();

		/// <summary>This stores all active and enabled instances in the open scenes.</summary>
		public static LinkedList<CwModel> Instances { get { return instances; } } private static LinkedList<CwModel> instances = new LinkedList<CwModel>(); private LinkedListNode<CwModel> instancesNode;

		private static MaterialPropertyBlock properties;

		public Renderer CachedRenderer
		{
			get
			{
				if (cachedRendererSet == false)
				{
					CacheRenderer();
				}

				return cachedRenderer;
			}
		}

		public Transform CachedTransform
		{
			get
			{
				return cachedTransform;
			}
		}

		public GameObject CachedGameObject
		{
			get
			{
				if (cachedRendererSet == false)
				{
					CacheRenderer();
				}

				return cachedGameObject;
			}
		}

		public bool Prepared
		{
			set
			{
				prepared = value;
			}

			get
			{
				return prepared;
			}
		}

		public abstract bool IsActivated
		{
			get;
		}

		public abstract void Activate();

		public abstract void RemoveComponents();

		public void ApplyTexture(CwSlot slot, Texture texture)
		{
			if (properties == null)
			{
				properties = new MaterialPropertyBlock();
			}

			if (cachedRendererSet == false)
			{
				CacheRenderer();
			}

			cachedRenderer.GetPropertyBlock(properties, slot.Index);

			properties.SetTexture(slot.Name, texture);

			cachedRenderer.SetPropertyBlock(properties, slot.Index);
		}

		public void ApplyTexture(Renderer r, CwSlot slot, Texture texture)
		{
			if (r != null)
			{
				if (properties == null)
				{
					properties = new MaterialPropertyBlock();
				}

				r.GetPropertyBlock(properties, slot.Index);

				properties.SetTexture(slot.Name, texture);

				r.SetPropertyBlock(properties, slot.Index);
			}
		}

		/// <summary>This will return a list of all paintables that overlap the specified bounds</summary>
		public static List<CwModel> FindOverlap(Vector3 position, float radius, int layerMask)
		{
			tempModels.Clear();

			foreach (var instance in instances)
			{
				if (CwHelper.IndexInMask(instance.gameObject.layer, layerMask) == true)
				{
					var bounds    = instance.CachedRenderer.bounds;
					var sqrRadius = radius + bounds.extents.magnitude; sqrRadius *= sqrRadius;

					if (Vector3.SqrMagnitude(position - bounds.center) < sqrRadius)
					{
						tempModels.Add(instance);

						if (instance.IsActivated == false)
						{
							instance.Activate();
						}
					}
				}
			}

			return tempModels;
		}

		public abstract List<CwPaintableTexture> FindPaintableTextures(CwGroup group);

		public abstract void GetPrepared(ref Mesh mesh, ref Matrix4x4 matrix, CwCoord coord);

		protected virtual void OnEnable()
		{
			instancesNode = instances.AddLast(this);

			cachedGameObject = gameObject;
			cachedTransform  = transform;
			cachedRenderer   = GetComponent<Renderer>();

			CwSerialization.TryRegister(this, hash);
		}

		protected virtual void OnDisable()
		{
			instances.Remove(instancesNode); instancesNode = null;
		}

		protected virtual void OnDestroy()
		{
			CwSerialization.TryRegister(this, default(CwHash));
		}

		protected virtual void CacheRenderer()
		{
			cachedRenderer    = GetComponent<Renderer>();
			cachedRendererSet = true;
		}

		/// <summary>This will scale the specified width and height values based on the current BaseScale setting.</summary>
		public void ScaleSize(ref int width, ref int height)
		{
			if (baseScale != Vector3.zero)
			{
				var scale = transform.localScale.magnitude / baseScale.magnitude;

				width  = Mathf.CeilToInt(width  * scale);
				height = Mathf.CeilToInt(height * scale);
			}
		}

		public Texture GetExistingTexture(CwSlot slot)
		{
			CachedRenderer.GetSharedMaterials(tempMaterials); // NOTE: Property

			if (slot.Index >= 0 && slot.Index < tempMaterials.Count)
			{
				var tempMaterial = tempMaterials[slot.Index];

				if (tempMaterial != null)
				{
					return tempMaterial.GetTexture(slot.Name);
				}
			}

			return null;
		}
	}
}