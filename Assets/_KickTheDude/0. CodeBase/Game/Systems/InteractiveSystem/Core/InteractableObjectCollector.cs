using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Sirenix.OdinInspector;
using Micosmo.SensorToolkit;

namespace Game.InteractiveSystem
{
    public class InteractableObjectCollector : SerializedMonoBehaviour
    {
        public event Action<IInteractable> ObjectCatched;
        public event Action<IInteractable> NearestObjectChanged;

        [SerializeField] private Sensor _itemSensor;
        [SerializeField] private List<IInteractable> _catchedObjects = new List<IInteractable>();

        [SerializeField] private bool _highlight = true;

        [SerializeField, ReadOnly] private IInteractable _nearestObject;

        public IEnumerable<IInteractable> CatchedObjects => _catchedObjects;
        public IInteractable NearestObject => _nearestObject;

        private void OnEnable()
        {
            _itemSensor.OnDetected.AddListener(ItemDetected);
            _itemSensor.OnLostDetection.AddListener(ItemLost);
        }

        private void OnDisable()
        {
            _itemSensor.OnDetected.RemoveListener(ItemDetected);
            _itemSensor.OnLostDetection.RemoveListener(ItemLost);

            UnhighlightAll();

            _catchedObjects.Clear();
        }

        private void ItemDetected(GameObject detectedObject, Sensor sensor)
        {
            var catchedInteractiveObject = detectedObject.GetComponentInParent<IInteractable>();

            if (catchedInteractiveObject == null) return;

            if (!_catchedObjects.Contains(catchedInteractiveObject))
            {
                _catchedObjects.Add(catchedInteractiveObject);
                SetNearestObject();
            }
        }

        private void ItemLost(GameObject detectedObject, Sensor sensor)
        {
            var catchedInteractiveObject = detectedObject.GetComponentInParent<IInteractable>();

            if (catchedInteractiveObject == null) return;

            if (_catchedObjects.Contains(catchedInteractiveObject))
            {
                if (_nearestObject == catchedInteractiveObject)
                    _nearestObject = null;

                _catchedObjects.Remove(catchedInteractiveObject);
                TrySetHighlightStatus(catchedInteractiveObject, false);

                SetNearestObject();
            }
        }

        private void LateUpdate()
        {
            SetNearestObject();
        }

        private void SetNearestObject()
        {
            if (_catchedObjects.Count == 0)
            {
                _nearestObject = null;

                NearestObjectChanged?.Invoke(null);
                return;
            }

            _catchedObjects.RemoveAll(x => x == null || x.Root == null);

            if (_catchedObjects.Count == 1 && _nearestObject == null)
            {
                var potentialNewNearestObject = _catchedObjects.First();
                if (_nearestObject != potentialNewNearestObject)
                {
                    _nearestObject = potentialNewNearestObject;

                    TrySetHighlightStatus(_nearestObject, true);

                    NearestObjectChanged?.Invoke(_nearestObject);
                }
                
                return;
            }

            foreach (var catchedObject in _catchedObjects)
            {
                if (catchedObject == null || catchedObject.Equals(null))
                {
                    //Debug.Log("continue catched null");
                    continue;
                }

                if (_nearestObject == null || catchedObject.Equals(null))
                {
                    //Debug.Log("continue nearest null");
                    continue;
                }

                if (catchedObject.Root == null) continue;
                if (_nearestObject.Root == null) continue;

                if (Vector3.Distance(transform.position, catchedObject.Root.position) 
                        < 
                        Vector3.Distance(transform.position, _nearestObject.Root.transform.position))
                {
                    TrySetHighlightStatus(_nearestObject, false);
                    _nearestObject = catchedObject;
                    TrySetHighlightStatus(_nearestObject, true);

                    NearestObjectChanged?.Invoke(_nearestObject);
                }
            }
        }

        private void TrySetHighlightStatus(IInteractable interactiveObject, bool highlighted)
        {
            if (!_highlight) return;
            if (interactiveObject == null) return;

            var highlighter = interactiveObject.Root.GetComponent<OutlineHighlighter>();

            if (highlighter)
            {
                if (highlighted)
                    highlighter.Highlight();
                else
                    highlighter.Deactivate();
            }
        }

        public void UnhighlightAll()
        {
            foreach (var catchedObject in _catchedObjects)
                TrySetHighlightStatus(catchedObject, false);
        }
    }
}
