using UnityEngine;
using UnityEngine.EventSystems;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to perform the Undo All action. This can be done by attaching it to a clickable object, or manually from the RedoAll method.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwButtonUndoAll")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Button Undo All")]
	public class CwButtonUndoAll : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
			UndoAll();
		}

		/// <summary>If you want to manually trigger UndoAll, then call this function.</summary>
		[ContextMenu("Undo All")]
		public void UndoAll()
		{
			CwStateManager.UndoAll();
		}

		protected virtual void Update()
		{
			var group = GetComponent<CanvasGroup>();

			if (group != null)
			{
				group.alpha = CwStateManager.CanUndo == true ? 1.0f : 0.5f;
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwButtonUndoAll;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwUndoAll_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component allows you to perform the Undo All action. This can be done by attaching it to a clickable object, or manually from the UndoAll method.");
		}
	}
}
#endif