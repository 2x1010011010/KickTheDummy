using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component automatically updates all CwModel and CwPaintableTexture instances at the end of the frame, batching all paint operations together.</summary>
	[DefaultExecutionOrder(100)]
	[DisallowMultipleComponent]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintableManager")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Paintable Manager")]
	public class CwPaintableManager : MonoBehaviour
	{
		/// <summary>This stores all active and enabled instances in the open scenes.</summary>
		public static LinkedList<CwPaintableManager> Instances { get { return instances; } } private static LinkedList<CwPaintableManager> instances = new LinkedList<CwPaintableManager>(); private LinkedListNode<CwPaintableManager> instancesNode;

		public static CwPaintableManager GetOrCreateInstance()
		{
			if (instances.Count == 0)
			{
				var paintableManager = new GameObject(typeof(CwPaintableManager).Name);

				//paintableManager.hideFlags = HideFlags.DontSave;

				paintableManager.AddComponent<CwPaintableManager>();
			}

			return instances.First.Value;
		}

		public static void SubmitAll(CwCommand command, Vector3 position, float radius, int layerMask, CwGroup group, CwModel targetModel, CwPaintableTexture targetTexture)
		{
			DoSubmitAll(command, position, radius, layerMask, group, targetModel, targetTexture);

			// Repeat paint?
			CwClone.BuildCloners();

			for (var c = 0; c < CwClone.ClonerCount; c++)
			{
				for (var m = 0; m < CwClone.MatrixCount; m++)
				{
					var copy = command.SpawnCopy();

					CwClone.Clone(copy, c, m);

					DoSubmitAll(copy, position, radius, layerMask, group, targetModel, targetTexture);

					copy.Pool();
				}
			}
		}

		private static void DoSubmitAll(CwCommand command, Vector3 position, float radius, int layerMask, CwGroup group, CwModel targetModel, CwPaintableTexture targetTexture)
		{
			if (targetModel != null)
			{
				if (targetTexture != null)
				{
					Submit(command, targetModel, targetTexture);
				}
				else
				{
					SubmitAll(command, targetModel, group);
				}
			}
			else
			{
				if (targetTexture != null)
				{
					Submit(command, targetTexture.Model, targetTexture);
				}
				else
				{
					SubmitAll(command, position, radius, layerMask, group);
				}
			}
		}

		private static void SubmitAll(CwCommand command, Vector3 position, float radius, int layerMask, CwGroup group)
		{
			var models = CwModel.FindOverlap(position, radius, layerMask);

			for (var i = models.Count - 1; i >= 0; i--)
			{
				SubmitAll(command, models[i], group);
			}
		}

		private static void SubmitAll(CwCommand command, CwModel model, CwGroup group)
		{
			var paintableTextures = model.FindPaintableTextures(group);

			for (var i = paintableTextures.Count - 1; i >= 0; i--)
			{
				Submit(command, model, paintableTextures[i]);
			}
		}

		public static CwCommand Submit(CwCommand command, CwModel model, CwPaintableTexture paintableTexture)
		{
			var copy = command.SpawnCopy();

			copy.Apply(paintableTexture);

			copy.Model   = model;
			copy.Submesh = paintableTexture.Slot.Index;

			paintableTexture.AddCommand(copy);

			return copy;
		}

		protected virtual void OnEnable()
		{
			instancesNode = instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			instances.Remove(instancesNode); instancesNode = null;
		}

		protected virtual void LateUpdate()
		{
			if (this == instances.First.Value && CwModel.Instances.Count > 0)
			{
				ClearAll();
				UpdateAll();
			}
			else
			{
				CwHelper.Destroy(gameObject);
			}
		}

		private void ClearAll()
		{
			foreach (var model in CwModel.Instances)
			{
				model.Prepared = false;
			}
		}

		private void UpdateAll()
		{
			foreach (var paintableTexture in CwPaintableTexture.Instances)
			{
				paintableTexture.ExecuteCommands(true, true);
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwPaintableManager;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPaintableManager_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component automatically updates all CwModel and CwPaintableTexture instances at the end of the frame, batching all paint operations together.");
		}
	}
}
#endif