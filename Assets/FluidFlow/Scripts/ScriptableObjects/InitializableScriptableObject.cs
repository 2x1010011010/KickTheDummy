using UnityEngine;

namespace FluidFlow
{
    /// ScriptableObject that is automatically initialized on game start
    public abstract class InitializableScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeChangeCallback;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeChangeCallback;
        }

        private void PlayModeChangeCallback(UnityEditor.PlayModeStateChange change)
        {
            if (change == UnityEditor.PlayModeStateChange.EnteredPlayMode) {
                Initialize();
            }
        }

#else
        protected virtual void Awake()
        {
            Initialize();
        }
#endif

        public abstract void Initialize();
    }
}