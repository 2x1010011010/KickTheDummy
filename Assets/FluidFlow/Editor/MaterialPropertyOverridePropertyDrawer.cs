using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(MaterialPropertyOverride))]
    public class MaterialPropertyOverridePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 3;
        }

        private static readonly GUIContent uvOverrideLabel = new GUIContent("Secondary UV Name");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shaderProperty = property.FindPropertyRelative("Target");
            var atlasTransformProperty = property.FindPropertyRelative("AtlasTransformPropertyName");
            var uvModeProperty = property.FindPropertyRelative("UVOverrideMode");
            var uvNameProperty = property.FindPropertyRelative("UVTargetName");

            System.Func<int, Rect> lineRect = (int index) => {
                var rect = position;
                rect.yMin = position.yMin + (EditorGUIUtility.singleLineHeight + 1) * index;
                rect.height = EditorGUIUtility.singleLineHeight;
                return rect;
            };

            using (new EditorGUI.PropertyScope(position, label, property)) {
                EditorGUI.PropertyField(lineRect(0), shaderProperty);
                EditorGUI.PropertyField(lineRect(1), atlasTransformProperty);

                var nameRect = lineRect(2);
                nameRect.xMax -= 90;
                EditorGUI.PropertyField(nameRect, uvNameProperty, uvOverrideLabel);
                var modeRect = lineRect(2);
                modeRect.xMin = modeRect.xMax - 90;
                using (new GUIIndentScope(0))
                    EditorGUI.PropertyField(modeRect, uvModeProperty, GUIContent.none);
            }
        }
    }
}
