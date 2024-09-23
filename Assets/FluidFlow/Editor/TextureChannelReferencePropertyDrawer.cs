using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(TextureChannelReference))]
    public class TextureChannelReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var modeProperty = property.FindPropertyRelative("mode");

            using (new EditorGUI.PropertyScope(position, label, property)) {
                position.height = EditorGUIUtility.singleLineHeight;
                if (label.text.Length > 0) {
                    EditorGUI.LabelField(position, label, GUIContent.none);
                    position.xMin += EditorGUIUtility.labelWidth;
                }

                using (new GUIIndentScope(0)) {
                    var rect = position;
                    rect.width = 75;
                    EditorGUI.PropertyField(rect, modeProperty, GUIContent.none);
                    position.xMin += rect.width;

                    if (TextureChannelReference.Mode.DIRECT == (TextureChannelReference.Mode)modeProperty.enumValueIndex) {
                        EditorUtil.PropertyFieldWithOptions(position, property.FindPropertyRelative("channel"), GUIContent.none, AssetReferenceEditorUtil.FindAssetFormats<TextureChannel>);
                    } else {
                        EditorUtil.OptionsTextField(position, property.FindPropertyRelative("identifier"), AssetReferenceEditorUtil.FindAssetNames<TextureChannel>);
                    }
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(TextureChannelFormatReference))]
    public class TextureChannelFormatReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var modeProperty = property.FindPropertyRelative("mode");

            using (new EditorGUI.PropertyScope(position, label, property)) {
                position.height = EditorGUIUtility.singleLineHeight;
                if (label.text.Length > 0) {
                    EditorGUI.LabelField(position, label, GUIContent.none);
                    position.xMin += EditorGUIUtility.labelWidth;
                }

                using (new GUIIndentScope(0)) {
                    var rect = position;
                    rect.width = 75;
                    EditorGUI.PropertyField(rect, modeProperty, GUIContent.none);
                    position.xMin += rect.width;

                    if (TextureChannelReference.Mode.DIRECT == (TextureChannelReference.Mode)modeProperty.enumValueIndex) {
                        EditorUtil.PropertyFieldWithOptions(position, property.FindPropertyRelative("format"), GUIContent.none, AssetReferenceEditorUtil.FindAssetFormats<TextureChannelFormat>);
                    } else {
                        EditorUtil.OptionsTextField(position, property.FindPropertyRelative("identifier"), AssetReferenceEditorUtil.FindAssetNames<TextureChannelFormat>);
                    }
                }
            }
        }


    }

    public static class AssetReferenceEditorUtil
    {
        public static List<System.Tuple<string, T>> FindAssetFormats<T>() where T : Object
        {
            var textureChannels = new List<System.Tuple<string, T>>();
            foreach (var channel in EditorUtil.EnumerateAllAssets<T>())
                textureChannels.Add(System.Tuple.Create(channel.name, channel));
            return textureChannels;
        }

        public static List<string> FindAssetNames<T>() where T : Object
        {
            var textureChannels = new List<string>();
            foreach (var channel in EditorUtil.EnumerateAllAssets<T>())
                textureChannels.Add(channel.name);
            return textureChannels;
        }
    }
}
