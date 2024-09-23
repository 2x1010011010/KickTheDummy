using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(FFDecal.Mask))]
    public class FFDecalMaskPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorUtil.PropertyComponentsField(position,
                            property.FindPropertyRelative("Texture"),
                            property.FindPropertyRelative("Components"),
                            label);
        }
    }

    [CustomPropertyDrawer(typeof(FFDecal.Channel))]
    public class FFDecalChannelPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var layout = new HorizontalLayout(position, 2, 1, 1, 3, 1);
            var name = property.FindPropertyRelative("TargetTextureChannel");
            var type = property.FindPropertyRelative("ChannelType");
            var source = property.FindPropertyRelative("Source");
            var data = property.FindPropertyRelative("Data");
            var mask = property.FindPropertyRelative("WriteMask");

            EditorUtil.FullsizePropertyField(layout.Get(0), name);
            EditorUtil.FullsizePropertyField(layout.Get(1), type);

            var sourceType = source.FindPropertyRelative("SourceType");
            EditorUtil.FullsizePropertyField(layout.Get(2), sourceType);

            EditorUtil.FullsizePropertyField(layout.Get(4), mask);

            var layout2 = (FFDecal.Channel.Type)type.enumValueIndex != FFDecal.Channel.Type.COLOR
                            ? new HorizontalLayout(layout.Get(3), 1, 1)
                            : new HorizontalLayout(layout.Get(3), 1);
            if ((FFDecal.ColorSource.Type)sourceType.enumValueIndex == FFDecal.ColorSource.Type.TEXTURE)
                EditorUtil.FullsizePropertyField(layout2.Get(0), source.FindPropertyRelative("Texture"));
            else
                EditorUtil.FullsizePropertyField(layout2.Get(0), source.FindPropertyRelative("Color"));

            switch ((FFDecal.Channel.Type)type.enumValueIndex) {
                case FFDecal.Channel.Type.NORMAL:
                    using (var labelWidth = new LabelWidthScope(52))
                        EditorGUI.PropertyField(layout2.Get(1), data, new GUIContent("Strength"));
                    break;

                case FFDecal.Channel.Type.FLUID:
                    using (var labelWidth = new LabelWidthScope(52))
                        EditorGUI.PropertyField(layout2.Get(1), data, new GUIContent("Amount"));
                    break;
            }
        }
    }
}