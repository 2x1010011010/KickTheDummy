using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    [PreferBinarySerialization]
    public class FFModelCache : InitializableScriptableObject
    {
        [System.Serializable]
        public struct SecondaryUVCacheData
        {
            public Mesh Source;
            public Mesh Cache;
        };

        [System.Serializable]
        public struct StitchCacheData
        {
            public Mesh Source;
            public Mesh GenerationSource; // same as Source for UV0, might be the secondary UV cache when using UV1
            public UVSet UVSet;
            public StitchResult[] Stitches;
        }
        public struct StitchCacheSetting
        {
            public Mesh Source;
            public UVSet UVSet;
        }

        [Tooltip("Target model asset the cache was generated for.")]
        [SerializeField] private GameObject target;
        public GameObject Target { get => target; }

        [SerializeField] private List<SecondaryUVCacheData> secondaryUVMeshCaches;
        public List<SecondaryUVCacheData> SecondaryUVMeshCaches { get => secondaryUVMeshCaches; }
        public List<Mesh> SecondaryUVMeshes() => secondaryUVMeshCaches.ConvertAll(data => data.Source);
        public bool TryGetSecondaryUVMesh(Mesh source, out Mesh cache)
        {
            var index = secondaryUVMeshCaches.FindIndex(data => data.Source == source);
            if (index < 0) {
                cache = null;
                return false;
            }
            cache = secondaryUVMeshCaches[index].Cache;
            return true;
        }

        [SerializeField] private List<StitchCacheData> stitchCaches;
        public List<StitchCacheData> StitchCaches { get => stitchCaches; }
        public List<StitchCacheSetting> StitchCacheSettings() => stitchCaches.ConvertAll(data => new StitchCacheSetting() { Source = data.Source, UVSet = data.UVSet });

        [System.NonSerialized] private bool initialized = false;
        public override void Initialize()
        {
            if (initialized)
                return;
            Cache.AddCache(this);
            initialized = true;
        }

#if UNITY_EDITOR
        [Tooltip("Unwrap parameters used for automatic secondary uv map generation, when the mesh currently has no secondary uv set.")]
        public UnwrapParamSerialized SecondaryUVUnwrapParameters;

        [Tooltip("Hash of the source model asset, to determine if the cache needs to be rebuild")]
        [SerializeField] private Hash128Serialized targetAssetHash;
        public Hash128 TargetHash { get => targetAssetHash; }

        public void SetTarget(GameObject target)
        {
            if (this.target) {
                Debug.LogErrorFormat("Target for '{0}' has already been set!", this);
                return;
            }
            this.target = target;
            targetAssetHash = FFEditorOnlyUtility.CalculateHashForAsset(target);
        }

        public bool SettingsMatch(List<Mesh> secondaryUVMeshes, List<StitchCacheSetting> stitchCacheSettings)
        {
            if (secondaryUVMeshes.Count != secondaryUVMeshCaches.Count || stitchCacheSettings.Count != stitchCaches.Count)
                return false;
            foreach (var cache in secondaryUVMeshCaches)
                if (!secondaryUVMeshes.Contains(cache.Source))
                    return false;
            foreach (var cache in stitchCaches)
                if (stitchCacheSettings.FindIndex(setting => setting.Source == cache.Source && setting.UVSet == cache.UVSet) < 0)
                    return false;
            return true;
        }

        public void ApplySettings(List<Mesh> secondaryUVMeshes, List<StitchCacheSetting> stitchCacheSettings)
        {
            using (var progress = new FFEditorOnlyUtility.ProgressBarScope("Updating FFModelCache")) {
                UpdateSecondaryUVCaches(secondaryUVMeshes, progress);
                UpdateStitchCaches(stitchCacheSettings, progress);
                targetAssetHash = FFEditorOnlyUtility.CalculateHashForAsset(target);    // update hash
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public void Regenerate() => ApplySettings(SecondaryUVMeshes(), StitchCacheSettings());

        private void UpdateSecondaryUVCaches(List<Mesh> settings, FFEditorOnlyUtility.ProgressBarScope progress)
        {
            for (var i = secondaryUVMeshCaches.Count - 1; i >= 0; i--)  // remove old caches, which are not used anymore
            {
                if (!settings.Contains(secondaryUVMeshCaches[i].Source)) {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(secondaryUVMeshCaches[i].Cache);
                    DestroyImmediate(secondaryUVMeshCaches[i].Cache);
                    secondaryUVMeshCaches.RemoveAt(i);
                }
            }

            for (var i = 0; i < settings.Count; i++) {
                if (!settings[i] || settings.FindIndex(0, i, previous => previous == settings[i]) >= 0) // skip nulls and repeats
                    continue;
                if (!settings[i].isReadable) {
                    Debug.LogErrorFormat("FluidFlow: {0} is not readable! Enable Read/Write in the import settings of the model.", settings[i]);
                    continue;
                }
                progress.Update($"Updating secondary uv mesh cache ({settings[i].name})", Mathf.Lerp(0, .5f, i / (float)settings.Count));
                AddSecondaryUVCache(settings[i]);
            }
        }

        private Mesh AddSecondaryUVCache(Mesh source)
        {
            var secondaryUVCache = Gravity.GenerateSecondaryUVCache(source, SecondaryUVUnwrapParameters);
            var existingCacheIndex = secondaryUVMeshCaches.FindIndex(cache => cache.Source == source);
            if (existingCacheIndex >= 0) {
                Utility.OverwriteExistingMesh(secondaryUVCache, secondaryUVMeshCaches[existingCacheIndex].Cache); // copy data to existing mesh, so all references keep valid
                UnityEditor.EditorUtility.SetDirty(secondaryUVMeshCaches[existingCacheIndex].Cache);
                DestroyImmediate(secondaryUVCache);
            } else {
                secondaryUVMeshCaches.Add(new SecondaryUVCacheData() { Source = source, Cache = secondaryUVCache });
                UnityEditor.AssetDatabase.AddObjectToAsset(secondaryUVCache, this);
            }
            return secondaryUVCache;
        }

        private void UpdateStitchCaches(List<StitchCacheSetting> settings, FFEditorOnlyUtility.ProgressBarScope progress)
        {
            stitchCaches.Clear();
            for (var i = 0; i < settings.Count; i++) {
                if (!settings[i].Source || settings.FindIndex(0, i, previous => previous.Source == settings[i].Source && previous.UVSet == settings[i].UVSet) >= 0) // skip nulls and repeats
                    continue;
                if (!settings[i].Source.isReadable) {
                    Debug.LogErrorFormat("FluidFlow: {0} is not readable! Enable Read/Write in the import settings of the model.", settings[i].Source);
                    continue;
                }
                progress.Update("Updating stitch cache (" + (i + 1) + "/" + settings.Count + ")", Mathf.Lerp(.5f, 1, (i + 1) / (float)settings.Count));

                var sourceMesh = settings[i].Source;
                if (settings[i].UVSet == UVSet.UV1 && !sourceMesh.HasUV1AndTransformations()) {
                    var cacheIndex = secondaryUVMeshCaches.FindIndex(cache => cache.Source == settings[i].Source);
                    sourceMesh = cacheIndex >= 0 ? secondaryUVMeshCaches[cacheIndex].Cache : AddSecondaryUVCache(sourceMesh);
                }
                using (var handle = Stitcher.Generate(sourceMesh, settings[i].UVSet)) {
                    handle.Handle.Complete();
                    stitchCaches.Add(new StitchCacheData() {
                        Source = settings[i].Source,
                        GenerationSource = sourceMesh,
                        UVSet = settings[i].UVSet,
                        Stitches = handle.Get().Stitches
                    });
                }
            }
        }
#endif
    }
}