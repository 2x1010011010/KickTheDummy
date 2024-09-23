using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(Updater))]
    public class UpdaterDrawer : PropertyDrawer
    {
        private const string modePropertyName = "UpdateMode";
        private const string intervalPropertyName = "FixedUpdateInterval";

        private bool InFixedMode(SerializedProperty modeProp)
        {
            return !modeProp.hasMultipleDifferentValues && modeProp.enumValueIndex == (int)Updater.Mode.FIXED;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property)) {
                var modeProperty = property.FindPropertyRelative(modePropertyName);
                var rect = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(rect, modeProperty);
                if (InFixedMode(modeProperty)) {
                    rect.y += EditorGUIUtility.singleLineHeight;
                    using (var indented = new EditorGUI.IndentLevelScope(1))
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative(intervalPropertyName));
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (InFixedMode(property.FindPropertyRelative(modePropertyName)) ? 2 : 1);
        }
    }
}