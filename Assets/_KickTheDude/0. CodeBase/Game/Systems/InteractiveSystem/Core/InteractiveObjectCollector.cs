using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.InteractiveSystem
{
    public class InteractiveObjectCollector : SerializedMonoBehaviour
    {
        public event Action<IInteractive<IInteractable>> NearestObjectChanged;

        [SerializeField] private RaycastSensor _raycastSensor;

        [SerializeField, ReadOnly] private IInteractive<IInteractable> _nearestObject;

        public IInteractive<IInteractable> NearestObject => _nearestObject;

        private void OnEnable()
        {
            _raycastSensor.FoundedInteractiveChanged += FoundedInteractiveChanged;
        }

        private void OnDisable()
        {
            _raycastSensor.FoundedInteractiveChanged -= FoundedInteractiveChanged;
        }

        private void FoundedInteractiveChanged(IInteractive<IInteractable> interactive)
        {
            _nearestObject = interactive;

            NearestObjectChanged?.Invoke(_nearestObject);
        }
    }
}
