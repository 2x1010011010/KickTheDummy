using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    [SerializeField] private Rigidbody _selfRigidbody;
    [SerializeField] private Vector3 _centerOfMass;

    private void OnValidate()
    {
        if (_selfRigidbody == null) _selfRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _selfRigidbody.centerOfMass = _centerOfMass;
    }
}
