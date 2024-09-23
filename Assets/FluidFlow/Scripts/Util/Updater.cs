using UnityEngine;
using UnityEngine.Events;

namespace FluidFlow
{
    [System.Serializable]
    public class Updater
    {
        /// <summary>
        /// Limit maximum update calls per frame, when FIXED update mode is used
        /// </summary>
        private const int MaxUpdatesPerFrame = 4;

        public enum Mode
        {
            [Tooltip("Has to be updated manually from script.")]
            CUSTOM,

            [Tooltip("Called in a fixed time step.")]
            FIXED,

            [Tooltip("Called each frame.")]
            CONTINUOUS
        }

        [Tooltip("Sets when or how often an update is called.")]
        public Mode UpdateMode;

        [Min(.001f)]
        [Tooltip("Timestep (seconds) between update calls.")]
        public float FixedUpdateInterval = .016f;

        private event UnityAction onUpdate = delegate { };

        private float lastUpdate = 0;

        public Updater(Mode mode, float fixedUpdateInterval = 0)
        {
            UpdateMode = mode;
            FixedUpdateInterval = fixedUpdateInterval;
        }

        public void RandomizeTimer()
        {
            lastUpdate = Random.value * FixedUpdateInterval;
        }

        public void AddListener(UnityAction callback)
        {
            onUpdate += callback;
        }

        public void Update()
        {
            switch (UpdateMode) {
                case Mode.CONTINUOUS:
                    onUpdate.Invoke();
                    break;

                case Mode.FIXED:
                    lastUpdate += Time.deltaTime;
                    for (int i = 0; lastUpdate > FixedUpdateInterval && i < MaxUpdatesPerFrame; i++) {
                        lastUpdate -= FixedUpdateInterval;
                        onUpdate.Invoke();
                    }
                    break;
            }
        }

        public void Invoke()
        {
            onUpdate.Invoke();
        }
    }
}