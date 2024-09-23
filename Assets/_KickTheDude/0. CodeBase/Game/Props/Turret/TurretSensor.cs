using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class TurretSensor : MonoBehaviour
{
    public event UnityAction<Transform> OnTargetDetected;
    public event UnityAction OnDetectionLose;

    [SerializeField, BoxGroup("RAYCAST SETUP")] private LayerMask _layerMask;
    [SerializeField, BoxGroup("RAYCAST SETUP")] private Transform _origin;
    [SerializeField, BoxGroup("RAYCAST SETUP")] private float _raycastDistance;

    private Ray _ray;
    private RaycastHit _hit = new RaycastHit();
    private bool _isTargetDetected;

    private void FixedUpdate()
    {
        Physics.Raycast(_origin.position, -transform.forward, out _hit, _raycastDistance, _layerMask, QueryTriggerInteraction.Ignore);

        if (_hit.collider == null)
        {
            if (!_isTargetDetected) return;
            _isTargetDetected = false;
            OnDetectionLose?.Invoke();
            return;
        }

        if (_hit.collider.attachedRigidbody != null)
        {
            OnTargetDetected?.Invoke(_hit.collider.attachedRigidbody.transform);
            _isTargetDetected = true;
        }
    }
}
