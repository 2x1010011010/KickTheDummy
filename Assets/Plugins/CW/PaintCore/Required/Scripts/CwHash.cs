using UnityEngine;

namespace PaintCore
{
	[System.Serializable]
    public struct CwHash
    {
		[SerializeField]
        private int v;

        public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return v.GetHashCode();
		}

		public CwHash(int newValue)
		{
			v = newValue;
		}

		public static implicit operator int(CwHash hash)
		{
			return hash.v;
		}

		public static implicit operator CwHash(int index)
		{
			return new CwHash(index);
		}

		public override string ToString()
		{
			return v.ToString();
		}
    }
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;

	[CustomPropertyDrawer(typeof(CwHash))]
	public class CwHashDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			property = property.FindPropertyRelative("v");

			var rect1 = position; rect1.xMax = position.xMax - 20;
			var rect2 = position; rect2.xMin = position.xMax - 18;

			EditorGUI.PropertyField(rect1, property, label);

			if (GUI.Button(rect2, "R") == true)
			{
				var path    = property.propertyPath;
				var objects = property.serializedObject.targetObjects;
				var context = property.serializedObject.context;

				for (var i = objects.Length - 1; i >= 0; i--)
				{
					var obj = new SerializedObject(objects[i], context);
					var pro = obj.FindProperty(path);

					pro.intValue = Random.Range(int.MinValue, int.MaxValue);

					obj.ApplyModifiedProperties();
				}
			}
		}
	}
}
#endif