using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(Tracked<>))]
    public class TrackedReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property)) {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("inspectorValue"), label);
            }
        }
    }
}

