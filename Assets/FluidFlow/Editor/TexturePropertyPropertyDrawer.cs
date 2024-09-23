using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(TextureProperty))]
    public class TexturePropertyPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shaderProperty = property.FindPropertyRelative("Shader");
            var nameProperty = property.FindPropertyRelative("PropertyName");

            using (new EditorGUI.PropertyScope(position, label, property)) {
                var rect = position;
                rect.y += 1;
                rect.width = rect.width - rect.width / 2 - 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, shaderProperty, GUIContent.none);
                rect.x += rect.width + 4;

                EditorUtil.OptionsTextField(rect, nameProperty, () => {
                    var shader = shaderProperty.FindPropertyRelative("value")?.objectReferenceValue as Shader;
                    var list = new List<string>();
                    if (shader) {
                        var count = shader.GetPropertyCount();
                        for (var i = 0; i < count; i++)
                            if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                                list.Add(shader.GetPropertyName(i));
                    }
                    return list;
                });
            }
        }
    }
}

