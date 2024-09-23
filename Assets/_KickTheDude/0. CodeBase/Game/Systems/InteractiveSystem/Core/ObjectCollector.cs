using Micosmo.SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectCollector<T> : MonoBehaviour
{
    public event Action<T> ObjectCatched;
    public event Action<T> NearestObjectChanged;

    [SerializeField] private Sensor _itemSensor;

    [SerializeField] private T _nearestObject;
    [SerializeField] private List<T> _catchedObjects = new List<T>();

    public IEnumerable<T> CatchedObjects => _catchedObjects;
    public T NearestObject => _nearestObject;

    private void OnEnable()
    {
        _itemSensor.OnDetected.AddListener(ItemDetected);
        _itemSensor.OnLostDetection.AddListener(ItemLost);
    }

    private void OnDisable()
    {
        _itemSensor.OnDetected.RemoveListener(ItemDetected);
        _itemSensor.OnDetected.RemoveListener(ItemLost);

        _catchedObjects.Clear();
    }

    private void ItemDetected(GameObject detectedObject, Sensor sensor)
    {
        var catchedInteractiveObject = detectedObject.GetComponentInParent<T>();

        if (catchedInteractiveObject == null) return;

        if (!_catchedObjects.Contains(catchedInteractiveObject))
        {
            _catchedObjects.Add(catchedInteractiveObject);
            SetNearestObject();
        }
    }

    private void ItemLost(GameObject detectedObject, Sensor sensor)
    {
        var catchedInteractiveObject = detectedObject.GetComponentInParent<T>();

        if (catchedInteractiveObject == null) return;

        if (_catchedObjects.Contains(catchedInteractiveObject))
        {
            _catchedObjects.Remove(catchedInteractiveObject);
            SetNearestObject();
        }
    }

    private void Update()
    {
        SetNearestObject();
    }

    private void SetNearestObject()
    {
        if (_catchedObjects.Count == 0)
        {
            _nearestObject = default;
            return;
        }

        if (_catchedObjects.Count == 1 && _nearestObject == null)
        {
            _nearestObject = _catchedObjects.First();
            return;
        }

        foreach (var catchedObject in _catchedObjects)
        {
            return;
            /*
            if (Vector3.Distance(transform.position, catchedObject.transform.position) < Vector3.Distance(transform.position, _nearestObject.transform.position))
            {
                _nearestObject = catchedObject;
            }
            */
        }
    }
}
