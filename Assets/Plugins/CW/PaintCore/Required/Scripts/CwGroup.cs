using UnityEngine;
using System.Linq;
using CW.Common;

namespace PaintCore
{
	/// <summary>This struct allows you to specify a group index with a group dropdown selector.</summary>
	[System.Serializable]
	public struct CwGroup
	{
		[SerializeField]
		private int index;

		public CwGroup(int newIndex)
		{
			index = newIndex;
		}

		public static implicit operator int(CwGroup group)
		{
			return group.index;
		}

		public static implicit operator CwGroup(int index)
		{
			return new CwGroup(index);
		}

		public override string ToString()
		{
			return index.ToString();
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(CwGroup))]
	public partial class CwGroup_Drawer : PropertyDrawer
	{
		public static void Draw(Rect position, SerializedProperty property)
		{
			var sPro      = property.FindPropertyRelative("index");
			var groupData = CwGroupData_Editor.GetGroupData(sPro.intValue);

			CwEditor.BeginError(groupData == null);
				if (GUI.Button(position, groupData != null ? groupData.name : "MISSING: " + sPro.intValue, EditorStyles.popup) == true)
				{
					var menu       = new GenericMenu();
					var groupDatas = CwGroupData_Editor.CachedInstances.OrderBy(d => d.Index);

					foreach (var cachedGroupData in groupDatas)
					{
						if (cachedGroupData != null)
						{
							AddMenuItem(menu, cachedGroupData, sPro, cachedGroupData.Index);
						}
					}

					menu.DropDown(position);
				}
			CwEditor.EndError();
		}

		private static void AddMenuItem(GenericMenu menu, CwGroupData groupData, SerializedProperty sPro, int index)
		{
			var content = new GUIContent(groupData.GetName(true));

			menu.AddItem(content, sPro.intValue == index, () => { sPro.intValue = index; sPro.serializedObject.ApplyModifiedProperties(); });
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var right = position; right.xMin += EditorGUIUtility.labelWidth;

			EditorGUI.LabelField(position, label);

			Draw(right, property);
		}
	}
}
#endif