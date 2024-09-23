using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(SurfaceDescriptor))]
    public class SurfaceDescriptorDrawer : PropertyDrawer
    {
        public static Dictionary<string, ReorderableList> Lists = new Dictionary<string, ReorderableList>();

        public static ReorderableList GetOrCreateSurfaceDescriptorEditList(SerializedProperty surfaceDescriptorProp)
        {
            var submeshDescriptorsProp = surfaceDescriptorProp.FindPropertyRelative("SubmeshDescriptors");
            if (!Lists.TryGetValue(surfaceDescriptorProp.propertyPath, out var list)) {
                list = new ReorderableList(surfaceDescriptorProp.serializedObject, submeshDescriptorsProp, true, false, true, true) {
                    onCanRemoveCallback = (list) => list.count > 1,
                    headerHeight = 0,
                };
                Lists.Add(surfaceDescriptorProp.propertyPath, list);
            }
            list.serializedProperty = submeshDescriptorsProp;
            return list;
        }

        public static ReorderableList CreateSurfaceDescriptorsListEdit(SerializedObject canvasSO)
        {
            var surfaceDescriptorsProp = canvasSO.FindProperty("SurfaceDescriptors");
            return new ReorderableList(canvasSO, surfaceDescriptorsProp, true, true, true, true) {
                elementHeightCallback = (int index) => EditorGUI.GetPropertyHeight(surfaceDescriptorsProp.GetArrayElementAtIndex(index)),
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 1, 1, 2, 2, 1, 1);
                    EditorGUI.LabelField(layout.Get(0), "Renderer", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "UV", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(2), "Submesh", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(3), "Offset", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(4), "Scale", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(5), "Aspect", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    EditorUtil.FullsizePropertyField(rect, surfaceDescriptorsProp.GetArrayElementAtIndex(index));
                },
                onCanRemoveCallback = (list) => list.count > 1
            };
        }

        private Mesh GetTargetMesh(SerializedProperty rendererProp)
        {
            var renderer = rendererProp.objectReferenceValue as Renderer;
            if (!renderer)
                return null;
            return renderer.GetMesh();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property)) {
                property.serializedObject.ApplyModifiedProperties();    // workaround, so the surface descriptor values are updated properly. Apply previously made, unapplied changes before update, otherwise they would be lost
                property.serializedObject.Update();
                var layout = new HorizontalLayout(position, 1, 1, 6);
                var rendererProp = property.FindPropertyRelative("Renderer");
                var cacheProp = property.FindPropertyRelative("Cache");
                var uvSetProp = property.FindPropertyRelative("UVSet");

                var rendererPropRect = layout.Get(0);
                EditorUtil.FullsizePropertyField(rendererPropRect, rendererProp);
                var targetMesh = GetTargetMesh(rendererProp);
                if (!targetMesh)
                    return;
                rendererPropRect.y += EditorGUIUtility.singleLineHeight + 4;
                rendererPropRect.xMax = layout.Get(1).xMax;
                using (new GUIHighlightScope(!cacheProp.objectReferenceValue, new Color(1, 1, 1, .4f), new Color(1, 1, 1, .4f)))
                    EditorUtil.FullsizePropertyField(rendererPropRect, cacheProp);
                EditorUtil.FullsizePropertyField(layout.Get(1), uvSetProp);

                var list = GetOrCreateSurfaceDescriptorEditList(property);
                list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    var layout = new HorizontalLayout(rect, 2, 2, 1, 1);
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    var submeshProp = element.FindPropertyRelative("SubmeshMask");
                    using (new GUIHighlightScope(submeshProp.intValue == 0, Color.red)) {
                        EditorUtil.SubmeshMaskField(layout.Get(0), submeshProp, targetMesh);
                    }
                    EditorUtil.FullsizePropertyField(layout.Get(1), element.FindPropertyRelative("UVOffset"));

                    var uvScaleProp = element.FindPropertyRelative("UVScale");
                    using (new GUIHighlightScope(uvScaleProp.floatValue == 0, Color.red))
                        EditorUtil.FullsizePropertyField(layout.Get(2), uvScaleProp);

                    EditorUtil.FullsizePropertyField(layout.Get(3), element.FindPropertyRelative("UVAspect"));

                };
                list.DoList(layout.Get(2));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Lists.TryGetValue(property.propertyPath, out var list))
                return list.GetHeight() + 1;
            else
                return EditorGUIUtility.singleLineHeight * 2 + 1;
        }
    }
}