using UnityEngine;
using UnityEngine.EventSystems;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to perform the Redo All action. This can be done by attaching it to a clickable object, or manually from the RedoAll method.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwButtonRedoAll")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Button Redo All")]
	public class CwButtonRedoAll : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			RedoAll();
		}

		/// <summary>If you want to manually trigger RedoAll, then call this function.</summary>
		[ContextMenu("Redo All")]
		public void RedoAll()
		{
			CwStateManager.RedoAll();
		}

		protected virtual void Update()
		{
			var group = GetComponent<CanvasGroup>();

			if (group != null)
			{
				group.alpha = CwStateManager.CanRedo == true ? 1.0f : 0.5f;
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwButtonRedoAll;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwRedoAll_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component allows you to perform the Redo All action. This can be done by attaching it to a clickable object, or manually from the RedoAll method.");
		}
	}
}
#endif