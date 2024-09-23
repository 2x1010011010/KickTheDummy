using UnityEngine;
using UnityEngine.EventSystems;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to perform the Clear action. This can be done by attaching it to a clickable object, or manually from the ClearAll method.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwButtonClearAll")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Button Clear All")]
	public class CwButtonClearAll : MonoBehaviour, IPointerClickHandler
	{
		/// <summary>When clearing a texture, should its undo states be cleared too?</summary>
		public bool ClearStates { set { clearStates = value; } get { return clearStates; } } [SerializeField] private bool clearStates = true;

		public void OnPointerClick(PointerEventData eventData)
		{
			ClearAll();
		}

		[ContextMenu("Clear All")]
		public void ClearAll()
		{
			foreach (var paintableTexture in CwPaintableTexture.Instances)
			{
				paintableTexture.Clear();

				if (clearStates == true)
				{
					paintableTexture.ClearStates();
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwButtonClearAll;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwButtonClearAll_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("clearStates", "When clearing a texture, should its undo states be cleared too?");
		}
	}
}
#endif