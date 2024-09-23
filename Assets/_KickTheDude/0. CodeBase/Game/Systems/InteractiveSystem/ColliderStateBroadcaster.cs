using UnityEngine;

namespace Game.InteractiveSystem
{
    public class ColliderStateBroadcaster : MonoBehaviour
    {
        public delegate void Disabled(Collider collider);
        public event Disabled OnDisabled;

        public delegate void Destroyed(Collider collider);
        public event Destroyed OnDestroyed;

        [SerializeField] private Collider _selfCollider;

        public Collider SelfCollider => _selfCollider;

        public void Init(Collider targetCollider)
        {
            _selfCollider = targetCollider;
        }

        private void OnValidate()
        {
            if (_selfCollider == null) _selfCollider = GetComponent<Collider>();
        }

        public void TryNotifyAboutDisabled()
        {
            if(!_selfCollider.enabled) OnDisabled?.Invoke(_selfCollider);
        }

        private void OnDisable()
        {
            OnDisabled?.Invoke(_selfCollider);
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(_selfCollider);
        }
    }
}