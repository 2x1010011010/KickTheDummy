using Infrastructure.Services.InputService;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

namespace CameraSystem
{
  public sealed class CameraMover : MonoBehaviour
  {
    [SerializeField] private CameraSettings _settings;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private MobileCameraController _leftSide;
    [SerializeField] private MobileCameraController _rightSide;
    [SerializeField] private float _maxAngle = 40f;
    //private IInputService _inputService;
    private float _speed;
    private float _sensitivity;
    private Vector3 _newPosition;
    private float _rotationOffset;
    private float _rotationX = 0f;
    private float _rotationY = 0f;
    
    private void Start()
    {
      //_inputService = inputService;
      _speed = _settings.MovementSpeed;
      _sensitivity = _settings.RotationSpeed;
      _rotationOffset = _settings.RotationOffset;
      _newPosition = transform.position;
    }

    private void Update()
    {
      if (!_inputPanel.activeSelf)
        return;
      
      Move();
      Rotate();
    }

    private void Move()
    {
      if (!_leftSide.OnPressed) return;
      
      var direction = GetMoveDirection();
      var forwardMovement = transform.forward * direction.z;
      var sideMovement = transform.right * direction.x;
      transform.position += (forwardMovement + sideMovement) * (_speed * Time.deltaTime);
    }
    
    private void Rotate()
    {
      if (!_rightSide.OnPressed) return;
      
      var direction = GetRotationDirection();
      
      _rotationX -= direction.z * _sensitivity * Time.deltaTime;
      _rotationX = Mathf.Clamp(_rotationX, -_maxAngle, _maxAngle);
      _rotationY += direction.x * _sensitivity * Time.deltaTime;
      transform.eulerAngles = new Vector3(_rotationX, _rotationY, 0);
    }
    
    private Vector3 GetMoveDirection()
    {
      foreach (var touch in Input.touches)
      {
        if (touch.fingerId != _leftSide.FingerID)
          continue;

        switch (touch.phase)
        {
          case TouchPhase.Moved:
            return new Vector3(-touch.deltaPosition.x,0, touch.deltaPosition.y).normalized;
          case TouchPhase.Stationary:
            return Vector3.zero;
        }
      }
      
      return Vector3.zero;
    }

    private Vector3 GetRotationDirection()
    {
      foreach (var touch in Input.touches)
      {
        if (touch.fingerId != _rightSide.FingerID)
          continue;

        switch (touch.phase)
        {
          case TouchPhase.Moved:
            return new Vector3(touch.deltaPosition.x,0, touch.deltaPosition.y).normalized;
          case TouchPhase.Stationary:
            return Vector3.zero;
        }
      }
      return Vector3.zero;
    }
  }
}