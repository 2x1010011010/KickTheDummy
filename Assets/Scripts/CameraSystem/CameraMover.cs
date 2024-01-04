using Infrastructure.Services.InputService;
using Tools.Weapon.Melee;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
using static UnityEngine.Screen;

namespace CameraSystem
{
  public sealed class CameraMover : MonoBehaviour
  {
    [SerializeField] private CameraSettings _settings;
    [SerializeField] private GameObject _inputPanel;
    [SerializeField] private MobileCameraController _leftSide;
    [SerializeField] private MobileCameraController _rightSide;
    [SerializeField] private float _maxAngle = 40f;
    [SerializeField] private Hand _handTool;
    //private IInputService _inputService;
    private float _speed;
    private float _sensitivity;
    private Vector3 _newPosition;
    private float _rotationOffset;
    private float _rotationX = 0f;
    private float _rotationY = 0f;
    private bool _isCharacterDragged;
    private float _previousMouseX;
    public bool MouseDown { get; set; }

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
      Move();
      Rotate();
    }

    private void HandleFirstClick()
    {
      if (Input.GetKeyDown(KeyCode.Mouse0))
      {
        MouseDown = true;
      }

      if (Input.GetKeyUp(KeyCode.Mouse0))
      {
        MouseDown = false;
      }
    }

    private void Move()
    {
      var direction = GetMoveDirection();
      var forwardMovement = transform.forward * direction.z;
      var sideMovement = transform.right * direction.x;
      transform.position += (forwardMovement + sideMovement) * (_speed * Time.deltaTime);
    }
    
    private void Rotate()
    {
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
        if (touch.rawPosition.x > width/2)
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
        if (touch.rawPosition.x < width/2)
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