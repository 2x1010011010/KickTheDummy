using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    /// <summary>
    /// UNITY EDITOR ONLY!
    /// </summary>
    [InitializeOnLoadAttribute]
    public static class FFModelCacheUpdater
    {
        static FFModelCacheUpdater()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && FFEditorOnlyUtility.AutoUpdateModelCaches) {
                UpdateAllCaches();
            }
        }

        public static void UpdateAllCaches()
        {
            using (var progress = new FFEditorOnlyUtility.ProgressBarScope("Updating FFModelCaches", "Collecting FFModelCache assets..")) {
                var guids = FindCacheAssetGUIDs();
                int i = 0;
                foreach (var cache in EnumerateCaches(guids)) {
                    progress.Update(cache.ToString(), (++i) / (float)guids.Length);
                    if (cache.Target) {
                        var hash = FFEditorOnlyUtility.CalculateHashForAsset(cache.Target);
                        if (hash != cache.TargetHash)
                            cache.Regenerate();
                    }
                }
                AssetDatabase.SaveAssets();
            }
        }

        private static string[] FindCacheAssetGUIDs()
        {
            return AssetDatabase.FindAssets("t:" + typeof(FFModelCache).Name);
        }

        private static IEnumerable<FFModelCache> EnumerateCaches(string[] guids)
        {
            foreach (var guid in guids) {
                yield return AssetDatabase.LoadAssetAtPath<FFModelCache>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
    }
}