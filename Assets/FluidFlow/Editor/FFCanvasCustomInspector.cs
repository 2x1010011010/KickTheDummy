using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FluidFlow
{
    [CustomEditor(typeof(FFCanvas), true)]
    [CanEditMultipleObjects]
    public class FFCanvasCustomInspector : Editor
    {
        private ReorderableList renderersList;
        private ReorderableList texturesList;
        private SerializedProperty autoInitializeProperty;
        private SerializedProperty initializeAsyncProperty;
        private SerializedProperty texturesProperty;
        private SerializedProperty resolutionProperty;
        private SerializedProperty materialOverridesProperty;

        private EditorZoomPanArea zoomArea;

        private bool EnableTexturePreview()
        {
            var canvas = target as FFCanvas;
            return canvas ? canvas.Initialized : false;
        }

        private void OnEnable()
        {
            autoInitializeProperty = serializedObject.FindProperty("AutoInitialize");
            initializeAsyncProperty = serializedObject.FindProperty("InitializeAsync");
            texturesProperty = serializedObject.FindProperty("TextureChannelDescriptors");
            resolutionProperty = serializedObject.FindProperty("Resolution");
            materialOverridesProperty = serializedObject.FindProperty("MaterialPropertyOverrides");

            zoomArea = new EditorZoomPanArea(new Vector2(.1f, 1000));

            // Set up the reorderable list
            SurfaceDescriptorDrawer.Lists.Clear();
            renderersList = SurfaceDescriptorDrawer.CreateSurfaceDescriptorsListEdit(serializedObject);
            texturesList = new ReorderableList(serializedObject, texturesProperty, true, true, true, true) {
                elementHeightCallback = (int index) => {
                    var element = texturesProperty.GetArrayElementAtIndex(index);
                    return (EnableTexturePreview() && element.isExpanded ? 300 : 0) + EditorUtil.ListElementHeight;
                },
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 2, 2, 1);
                    EditorGUI.LabelField(layout.Get(0), "Texture Channel", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "Format", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(2), "Initialization", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = texturesProperty.GetArrayElementAtIndex(index);
                    var firstLine = rect;
                    firstLine.y += EditorUtil.ListElementPadding;
                    firstLine.height = EditorGUIUtility.singleLineHeight;

                    if (EnableTexturePreview()) {
                        firstLine.xMin += 10;
                        element.isExpanded = EditorGUI.Foldout(firstLine, element.isExpanded, GUIContent.none);
                        if (element.isExpanded) {
                            var canvas = target as FFCanvas;
                            var textureChannelRef = canvas.TextureChannelDescriptors[index].TextureChannelReference;
                            if (textureChannelRef.IsValid && canvas.TextureChannels.ContainsKey(textureChannelRef)) {
                                var texture = canvas.TextureChannels[textureChannelRef.Resolve()];
                                var position = rect;
                                position.yMin += EditorUtil.ListElementHeight;
                                var size = Mathf.Min(position.width, position.height);
                                zoomArea.Draw(position, new Vector2(size, size), () => {
                                    using (texture.SetTemporaryFilterMode(FilterMode.Point))
                                        EditorGUI.DrawTextureTransparent(new Rect(0, 0, size, size), texture, ScaleMode.ScaleToFit);
                                }, false);
                                position.yMin = position.yMax - EditorGUIUtility.singleLineHeight;
                                EditorGUI.LabelField(position, texture.graphicsFormat.ToString(), EditorStyles.miniLabel);
                            }
                        }
                    }

                    var layout = new HorizontalLayout(firstLine, 2, 2, 1);
                    EditorUtil.FullsizePropertyField(layout.Get(0), element.FindPropertyRelative("TextureChannelReference"));
                    EditorUtil.FullsizePropertyField(layout.Get(1), element.FindPropertyRelative("FormatReference"));
                    EditorUtil.FullsizePropertyField(layout.Get(2), element.FindPropertyRelative("Initialization"));
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(autoInitializeProperty);
            EditorGUILayout.PropertyField(initializeAsyncProperty);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("Surfaces", EditorStyles.boldLabel);
                if (GUILayout.Button("Open In Editor", EditorStyles.miniButton))
                    FFCanvasLayoutEditor.Init(target as FFCanvas);
            }
            renderersList.displayRemove = renderersList.count > 1;
            renderersList.DoLayoutListIndented();

            EditorGUILayout.PropertyField(resolutionProperty);
            texturesList.DoLayoutListIndented();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(materialOverridesProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}