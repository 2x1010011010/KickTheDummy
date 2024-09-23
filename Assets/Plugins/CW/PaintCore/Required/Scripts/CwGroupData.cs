using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This object allows you to define information about a paint group like its name, which can then be selected using the <b>CwGroup</b> setting on components like <b>CwPaintableTexture</b> and <b>CwPaintDecal</b>.</summary>
	public class CwGroupData : ScriptableObject
	{
		/// <summary>This allows you to set the ID of this group (e.g. 100).
		/// NOTE: This number should be unique, and not shared by any other <b>CwGroupData</b>.</summary>
		public int Index { set { index = value; } get { return index; } } [SerializeField] private int index;

		/// <summary>This method allows you to get the <b>name</b> of the current group, with an optional prefix of the <b>Index</b> (e.g. "100: Albedo").</summary>
		public string GetName(bool prefixNumber)
		{
			if (prefixNumber == true)
			{
				return index + ": " + name;
			}

			return name;
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwGroupData;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwGroupData_Editor : CwEditor
	{
		private static List<CwGroupData> cachedInstances = new List<CwGroupData>();

		private static bool cachedInstancesSet;

		/// <summary>This static method calls <b>GetAlias</b> on the <b>CwGroupData</b> with the specified <b>Index</b> setting, or null.</summary>
		public static string GetGroupName(int index, bool prefixNumber)
		{
			var groupData = GetGroupData(index);

			return groupData != null ? groupData.GetName(prefixNumber) : null;
		}

		/// <summary>This static method returns the <b>CwGroupData</b> with the specified <b>Index</b> setting, or null.</summary>
		public static CwGroupData GetGroupData(int index)
		{
			foreach (var cachedGroupName in CachedInstances)
			{
				if (cachedGroupName != null && cachedGroupName.Index == index)
				{
					return cachedGroupName;
				}
			}

			return null;
		}

		/// <summary>This static method forces the cached instance list to update.
		/// NOTE: This does nothing in-game.</summary>
		public static void UpdateCachedInstances()
		{
			cachedInstancesSet = true;

			cachedInstances.Clear();

			foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:CwGroupData"))
			{
				var groupName = UnityEditor.AssetDatabase.LoadAssetAtPath<CwGroupData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));

				cachedInstances.Add(groupName);
			}
		}

		/// <summary>This static property returns a list of all cached <b>CwGroupData</b> instances.
		/// NOTE: This will be empty in-game.</summary>
		public static List<CwGroupData> CachedInstances
		{
			get
			{
				if (cachedInstancesSet == false)
				{
					UpdateCachedInstances();
				}

				return cachedInstances;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateCachedInstances();
		}

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var clashes = CachedInstances.Where(d => d.Index == tgt.Index);

			BeginError(clashes.Count() > 1);
				Draw("index", "This allows you to set the ID of this group (e.g. 100).\n\nNOTE: This number should be unique, and not shared by any other CwGroupData.");
			EndError();

			Separator();

			EditorGUILayout.LabelField("Current Groups", EditorStyles.boldLabel);

			var groupDatas = CachedInstances.OrderBy(d => d.Index);

			BeginDisabled(true);
				foreach (var groupData in groupDatas)
				{
					if (groupData != null)
					{
						EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField(groupData.name);
							EditorGUILayout.IntField(groupData.Index);
						EditorGUILayout.EndHorizontal();
					}
				}
			EndDisabled();
		}
	}
}
#endif