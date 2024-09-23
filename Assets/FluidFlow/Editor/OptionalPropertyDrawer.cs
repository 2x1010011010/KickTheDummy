using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");
            return EditorGUI.GetPropertyHeight(valueProperty);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");
            var enabledProperty = property.FindPropertyRelative("enabled");

            using (new EditorGUI.PropertyScope(position, label, property)) {

                using (new GUIEnableScope(enabledProperty.boolValue)) {
                    var valueRect = position;
                    valueRect.xMax -= (position.height + 2);
                    EditorGUI.PropertyField(valueRect, valueProperty, label, true);
                }

                using (new GUIIndentScope(0)) {
                    position.xMin = position.xMax - position.height;
                    EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
                }
            }
        }
    }
}