using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FluidFlow
{
    [CustomEditor(typeof(FFModelCache))]
    public class FFModelCacheInspector : Editor
    {
        private FFModelCache cache;

        private List<Mesh> secondaryUVMeshes;
        private ReorderableList secondaryUVMeshesList;

        private List<FFModelCache.StitchCacheSetting> stitchCacheSettings;
        private ReorderableList stitchCacheSettingsList;

        private SerializedProperty unwrapParamsProp;

        private Mesh[] sourceMeshes;

        private void OnEnable()
        {
            cache = target as FFModelCache;
            UpdateSettings();
            unwrapParamsProp = serializedObject.FindProperty("SecondaryUVUnwrapParameters");
        }

        private bool SettingsModified()
        {
            return !cache.SettingsMatch(secondaryUVMeshes, stitchCacheSettings);
        }

        private void OnDisable()
        {
            if (!IsCacheValid())
                return;
            if (SettingsModified()) {
                if (EditorUtility.DisplayDialog("FFModelCache", $"Apply changes to {cache}?", "Apply", "Revert"))
                    ApplyChanges();
            }
        }

        private void UpdateSettings()
        {
            if (!IsCacheValid())
                return;

            sourceMeshes = FFEditorOnlyUtility.GetSubObjectsOfType<Mesh>(cache.Target).ToArray();

            secondaryUVMeshes = cache.SecondaryUVMeshes();
            secondaryUVMeshesList = new ReorderableList(secondaryUVMeshes, typeof(Mesh), true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                onAddCallback = (ReorderableList list) => {
                    var popup = new GenericMenu();
                    void addElementCallback(object mesh) => secondaryUVMeshes.Add(mesh as Mesh);
                    foreach (var mesh in sourceMeshes)
                        popup.AddItem(new GUIContent(mesh.ToString()), false, addElementCallback, mesh);
                    popup.ShowAsContext();
                },
                drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "Secondary UV Meshes", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    using (new EditorGUI.DisabledGroupScope(true))
                        EditorGUI.ObjectField(rect, secondaryUVMeshes[index], typeof(Mesh), false);
                }
            };

            stitchCacheSettings = cache.StitchCacheSettings();
            stitchCacheSettingsList = new ReorderableList(stitchCacheSettings, typeof(FFModelCache.StitchCacheSetting), true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                onAddCallback = (ReorderableList list) => {
                    var popup = new GenericMenu();
                    void addElementCallback(object mesh) => stitchCacheSettings.Add(new FFModelCache.StitchCacheSetting() { Source = mesh as Mesh });
                    foreach (var mesh in sourceMeshes)
                        popup.AddItem(new GUIContent(mesh.ToString()), false, addElementCallback, mesh);
                    popup.ShowAsContext();
                },
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 1, 1);
                    EditorGUI.LabelField(layout.Get(0), "Mesh", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "UV", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    var item = stitchCacheSettings[index];
                    var layout = new HorizontalLayout(rect, 1, 1);
                    var localIndex = index;
                    EditorUtil.ObjectPopup(layout.Get(0), item.Source, sourceMeshes, (mesh) => {
                        var setting = stitchCacheSettings[localIndex];
                        setting.Source = mesh as Mesh;
                        stitchCacheSettings[localIndex] = setting;
                    });
                    item.UVSet = (UVSet)EditorGUI.EnumPopup(layout.Get(1), item.UVSet);
                    stitchCacheSettings[index] = item;
                }
            };
        }

        private static readonly GUIContent autoUpdateToggle = new GUIContent("Auto Update Before Play", "Ensure all caches are up-to-date before entering the play mode. A cache is only updated, if the hash of the target model has changed. Note that enabling this can add some overhead to entering the playmode.");
        private static readonly GUIContent updateCacheButton = new GUIContent("Update All Caches", "Update all caches in the current project, whose target models have been modified.");

        public override void OnInspectorGUI()
        {
            if (IsCacheValid()) {
                serializedObject.Update();
                using (new EditorGUILayout.HorizontalScope()) {
                    using (var disabled = new GUIEnableScope(false))
                        EditorGUILayout.ObjectField("Target", cache.Target, typeof(GameObject), false);
                    if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.MaxWidth(75)))
                        UpdateSettings();
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Secondary UV Caches", EditorStyles.boldLabel);
                secondaryUVMeshesList.DoLayoutList();

                EditorGUILayout.LabelField("Stitch Caches", EditorStyles.boldLabel);
                stitchCacheSettingsList.DoLayoutList();

                using (var disabled = new GUIEnableScope(secondaryUVMeshes.FindIndex(mesh => !mesh.HasUV1AndTransformations()) >= 0 || stitchCacheSettings.FindIndex(setting => setting.UVSet == UVSet.UV1 && !setting.Source.HasUV1AndTransformations()) >= 0))
                    EditorGUILayout.PropertyField(unwrapParamsProp);

                using (var h = new GUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (SettingsModified()) {
                        if (GUILayout.Button("Revert"))
                            UpdateSettings();
                        if (GUILayout.Button("Apply"))
                            ApplyChanges();
                    } else {
                        if (GUILayout.Button("Regenerate"))
                            ApplyChanges();
                    }
                }

                serializedObject.ApplyModifiedProperties();
            } else if (cache && !cache.Target) {
                EditorGUILayout.LabelField("Target has been removed.");
            }

            EditorGUILayout.Space();
            using (var header = new HeaderGroupScope($"{typeof(FFModelCache).Name} Global Settings")) {
                if (header.expanded) {
                    FFEditorOnlyUtility.AutoUpdateModelCaches = EditorGUILayout.Toggle(autoUpdateToggle, FFEditorOnlyUtility.AutoUpdateModelCaches);
                    if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), updateCacheButton))
                        FFModelCacheUpdater.UpdateAllCaches();
                }
            }
        }

        private void ApplyChanges()
        {
            cache.ApplySettings(secondaryUVMeshes, stitchCacheSettings);
            EditorUtility.SetDirty(cache);
            AssetDatabase.SaveAssets();
            UpdateSettings();
        }

        private bool IsCacheValid()
        {
            return cache && cache.Target;
        }
    }

    public static class FFModelCacheCreator
    {
        [MenuItem("Assets/Create/Fluid Flow/Model Cache")]
        public static void CreateCache()
        {
            CreateFromModel(Selection.activeGameObject);
        }

        [MenuItem("Assets/Create/Fluid Flow/Model Cache", validate = true)]
        public static bool ValidateCreateCache()
        {
            if (!Selection.activeGameObject)
                return false;
            return EditorUtil.IsModel(Selection.activeGameObject);
        }

        [MenuItem("CONTEXT/FFCanvas/Set Auto Update FFModelCaches")]
        [MenuItem("CONTEXT/FFModelCache/Set Auto Update FFModelCaches")]
        public static void SetAutoCacheUpdate()
        {
            FFEditorOnlyUtility.AutoUpdateModelCaches = EditorUtility.DisplayDialog("Auto Update FFModelCaches Before Play?",
                "Ensure all caches are up-to-date before entering the play mode. A cache is only updated, if the hash of the target model has changed. Note that enabling this can add some overhead to entering the playmode.",
                "Enable",
                "Disable");
        }

        public static void CreateFromModel(GameObject model)
        {
            if (!EditorUtil.IsModel(model)) {
                Debug.LogErrorFormat("Unable to create FluidModelCache for {0}. Not a model.", model.name);
                return;
            }

            var asset = ScriptableObject.CreateInstance<FFModelCache>();
            try {
                asset.SetTarget(model);
            } catch (FileLoadException e) {
                Debug.LogError(e.ToString());
                return;
            }

            var path = AssetDatabase.GetAssetPath(model);
            path = Path.Combine(Path.GetDirectoryName(path), model.name + "_Cache.asset");
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}