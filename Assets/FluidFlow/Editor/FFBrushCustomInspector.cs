using UnityEditor;
using UnityEngine;

namespace FluidFlow
{

    [CustomPropertyDrawer(typeof(FFBrush))]
    public class FFBrushPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var type = (FFBrush.Type)property.FindPropertyRelative("BrushType").enumValueIndex;
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing)
                * (type == FFBrush.Type.FLUID ? 4 : 3);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var typeProperty = property.FindPropertyRelative("BrushType");
            var colorProperty = property.FindPropertyRelative("Color");
            var dataProperty = property.FindPropertyRelative("Data");
            var fadeProperty = property.FindPropertyRelative("Fade");

            var singleLine = position;
            singleLine.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleLine, typeProperty, label);

            using (new EditorGUI.IndentLevelScope()) {
                singleLine.y = singleLine.yMax + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(singleLine, colorProperty);

                if ((FFBrush.Type)typeProperty.enumValueIndex == FFBrush.Type.FLUID) {
                    singleLine.y = singleLine.yMax + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(singleLine, dataProperty);
                }

                singleLine.y = singleLine.yMax + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(singleLine, fadeProperty);
            }
        }
    }
}