using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Sirenix.OdinInspector;

namespace Game.InteractiveSystem
{
    public class CollidersContainer : MonoBehaviour
    {
        [SerializeField, BoxGroup("SETUP")] private Transform _transformToFindColliders;
        [SerializeField, BoxGroup("SETUP")] private List<Collider> _collidersInContainer = new List<Collider>();
        [SerializeField, BoxGroup("SETUP")] private List<Collider> _triggersInContainer = new List<Collider>();

        [SerializeField, BoxGroup("PARAMETERS")] private bool _findDisabled;

        [SerializeField, BoxGroup("DEBUG")] private bool _drawBounds;
        [SerializeField, BoxGroup("DEBUG")] private Bounds _bounds;
        [SerializeField, ReadOnly, BoxGroup("DEBUG")] private bool _collidersDeactivated;
        [SerializeField, ReadOnly, BoxGroup("DEBUG")] private bool _triggersDeactivated;
        [SerializeField, ReadOnly, BoxGroup("DEBUG")] private bool _collidersIgnored;

        public IEnumerable<Collider> CollidersInContainer => _collidersInContainer;
        public IEnumerable<Collider> TriggersInContainer => _triggersInContainer;
        public Bounds Bounds => _bounds;

        private void OnValidate()
        {
            if (_transformToFindColliders == null) _transformToFindColliders = transform;
            if (_collidersInContainer.Count > 0 && _bounds.size == Vector3.zero) CalculateBounds();
        }

        private void Start()
        {
            AddColliderStateBroadcasters(_collidersInContainer);
            AddColliderStateBroadcasters(_triggersInContainer);
        }

        private void AddColliderStateBroadcasters(List<Collider> colliders)
        {
            foreach (Collider collider in colliders)
            {
                var colliderStateBroadcasters = collider.gameObject.GetComponents<ColliderStateBroadcaster>();
                ColliderStateBroadcaster newColliderStateBroadcaster = null;
                if (colliderStateBroadcasters.Length == 0)
                {
                    newColliderStateBroadcaster = collider.gameObject.AddComponent<ColliderStateBroadcaster>();
                    newColliderStateBroadcaster.Init(collider);
                }
                else
                {
                    bool needAdd = true;
                    foreach (var colliderStateBroadcaster in colliderStateBroadcasters)
                    {
                        if (colliderStateBroadcaster.SelfCollider == collider)
                        {
                            needAdd = false;
                        }
                    }

                    if (needAdd)
                    {
                        newColliderStateBroadcaster = collider.gameObject.AddComponent<ColliderStateBroadcaster>();
                        newColliderStateBroadcaster.Init(collider);
                    }
                }
            }
        }

        [Button(ButtonSizes.Large), BoxGroup("ACTIONS")]
        public void TryFindColliders()
        {
            ClearCollidersContainer();

            foreach (var collider in _transformToFindColliders.GetComponentsInChildren<Collider>(_findDisabled))
            {
                if (collider.isTrigger)
                {
                    _triggersInContainer.Add(collider);
                }
                else
                {
                    _collidersInContainer.Add(collider);
                }
            }

            CalculateBounds();
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }

        [Button(ButtonSizes.Large), BoxGroup("ACTIONS")]
        public void IgnoreSelf()
        {
            ChangeColliderIgnoreStatusWithOtherContainer(this, true);
            _collidersIgnored = true;
        }

        [Button(ButtonSizes.Large), BoxGroup("ACTIONS")]
        public void UnignoreSelf()
        {
            ChangeColliderIgnoreStatusWithOtherContainer(this, false);
            _collidersIgnored = false;
        }

        public void ClearCollidersContainer()
        {
            _collidersInContainer.Clear();
            _triggersInContainer.Clear();
        }

        public void ActivateAllColliders()
        {
            foreach (var collider in _collidersInContainer)
            {
                collider.enabled = true;
            }

            _collidersDeactivated = false;
        }

        public void DeactivateAllColliders()
        {
            foreach (var collider in _collidersInContainer)
            {
                collider.enabled = false;

                var colliderStateBroadcasters = collider.gameObject.GetComponents<ColliderStateBroadcaster>();

                foreach (var colliderStateBroadcaster in colliderStateBroadcasters)
                    if (colliderStateBroadcaster.SelfCollider == collider) colliderStateBroadcaster.TryNotifyAboutDisabled();
            }

            _collidersDeactivated = true;
        }

        public void ActivateAllTriggers()
        {
            foreach (var trigger in _triggersInContainer)
            {
                trigger.enabled = true;
            }

            _triggersDeactivated = false;
        }

        public void DeactivateAllTriggers()
        {
            foreach (var trigger in _triggersInContainer)
            {
                trigger.enabled = false;

                var colliderStateBroadcasters = trigger.gameObject.GetComponents<ColliderStateBroadcaster>();

                foreach (var colliderStateBroadcaster in colliderStateBroadcasters)
                    if (colliderStateBroadcaster.SelfCollider == trigger) colliderStateBroadcaster.TryNotifyAboutDisabled();
            }

            _triggersDeactivated = true;
        }

        public void ChangeColliderIgnoreStatusWithOtherContainer(CollidersContainer collidersContainer, bool ignoreCollisions)
        {
            foreach (var selfColliderInContainer in _collidersInContainer)
            {
                foreach (var otherColliderInContainer in collidersContainer.CollidersInContainer)
                {
                    Physics.IgnoreCollision(selfColliderInContainer, otherColliderInContainer, ignoreCollisions);
                }
            }
        }

        public void ChangeTriggerIgnoreStatusWithOtherContainer(CollidersContainer collidersContainer, bool ignoreCollisions)
        {
            foreach (var selfTriggerInContainer in _triggersInContainer)
            {
                foreach (var otherTriggerInContainer in collidersContainer.CollidersInContainer)
                {
                    Physics.IgnoreCollision(selfTriggerInContainer, otherTriggerInContainer, ignoreCollisions);
                }
            }
        }

        public bool ContainsCollider(Collider colliderForCheck)
        {
            return _collidersInContainer.Contains(colliderForCheck);
        }

        public Vector3 FindClosestPointOfColliders(Vector3 position)
        {
            var curentClosestPoint = _collidersInContainer.FirstOrDefault().ClosestPoint(position);

            foreach(var collider in _collidersInContainer)
            {
                var potentialClosestPoint = collider.ClosestPoint(position);
                if (Vector3.Distance(potentialClosestPoint, position) < Vector3.Distance(curentClosestPoint, position))
                {
                    curentClosestPoint = potentialClosestPoint;
                }
            }

            return curentClosestPoint;
        }

        public void CalculateBounds()
        {
            if (_collidersInContainer.Count == 0) return;

            var bounds = _collidersInContainer[0].bounds;

            if (_collidersInContainer.Count > 1)
            {
                for (int i = 1; i < _collidersInContainer.Count; i++)
                {
                    bounds.Encapsulate(_collidersInContainer[i].bounds);
                }
            }

            _bounds = bounds;

#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }

        private void OnDrawGizmos()
        {
            if (_drawBounds)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(_bounds.center, _bounds.extents * 2);
            }
        }
    }
}

