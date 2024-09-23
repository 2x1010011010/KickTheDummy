using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class TurretRotator : MonoBehaviour
{
    [SerializeField, Range(0, 360) ,BoxGroup("TURRET ROTATION")] private float _angle;
    [SerializeField, BoxGroup("TURRET ROTATION")] private float _rotationSpeed;
    [SerializeField, BoxGroup("TURRET ROTATION")] private float _pause = 0.01f;
    [SerializeField, BoxGroup("TURRET ROTATION")] private List<Transform> _rotateObjects;

    private int _direction = 1;
    private float _startRotation;
    private bool _isRotating = false;
    private Coroutine _rotating;
    private float _minLimitAngle;
    private float _maxLimitAngle;

    public void Init()
    {
        _startRotation = _rotateObjects[0].transform.eulerAngles.y;
        _minLimitAngle = _startRotation -_angle / 2;
        _maxLimitAngle = _startRotation + _angle / 2;
    }

    public void StartRotation()
    {
        if (_isRotating) return;

        _rotating = StartCoroutine(TurretRotation());
        _isRotating = true;
    }

    public void StopRotation()
    {
        if (!_isRotating) return;
        
        StopCoroutine(_rotating);
        _isRotating = false;
    }

    public void TurnTurretToTarget(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        _rotateObjects[0].rotation = Quaternion.Lerp(_rotateObjects[0].rotation, Quaternion.Euler(0, Mathf.Clamp(lookRotation.eulerAngles.y, _minLimitAngle, _maxLimitAngle), 0), Time.deltaTime * _rotationSpeed);
    }

    private IEnumerator TurretRotation()
    {
        var direction = 1;
            
        while (true)
        {
            foreach (var item in _rotateObjects)
            {
                item.transform.Rotate(Vector3.up * direction);
            }

            if ((int)_rotateObjects[0].transform.localEulerAngles.y < (int)_minLimitAngle)
                direction = 1;
            if ((int)_rotateObjects[0].transform.localEulerAngles.y > (int)_maxLimitAngle)
                direction = -1;
            
            
            yield return new WaitForSeconds(_pause);
        }
    }
}
