using Tools.Weapon.Melee;
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
    [SerializeField] private Hand _handTool;
    //private IInputService _inputService;
    private float _speed;
    private float _sensitivity;
    private Vector3 _newPosition;
    private Vector3 _rotationOffset;
    private float _rotationX = 0f;
    private float _rotationY = 0f;
    private bool _isCharacterDragged;
    private float _previousMouseX;
    private float _halfWidth;

    public bool MouseDown { get; set; }
    public bool IsCameraMoved { get; set; }


    private void Start()
    {
      //_inputService = inputService;
      _speed = _settings.MovementSpeed;
      _sensitivity = _settings.RotationSpeed;
      _rotationOffset = _settings.RotationOffset;
      _newPosition = transform.position;
      _halfWidth = Screen.width / 2.0f;
    }

    private void Update()
    {
      if (_handTool.Dragged) return;
      
      if (Input.touchCount == 0) return;
      
      if (Input.GetTouch(0).phase != TouchPhase.Moved) return;
      
      if (Input.GetTouch(0).deltaPosition.magnitude < _settings.DeltaPosition.magnitude) return;
      
      Move();
      Rotate();
    }
    

    private void Move()
    {
      var direction = GetMoveDirection();
      var forwardMovement = transform.forward * -direction.z;
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
        var touch = Input.GetTouch(0);
        
        if (touch.position.x > _halfWidth)
          return Vector3.zero;

        if (touch.deltaPosition.magnitude < _settings.DeltaPosition.magnitude) 
          return Vector3.zero;
        
        switch (touch.phase)
        {
          case TouchPhase.Moved:
            return new Vector3(touch.deltaPosition.x,0, -touch.deltaPosition.y).normalized;
          case TouchPhase.Stationary:
            return Vector3.zero;
        
        }
      
      return Vector3.zero;
    }

    private Vector3 GetRotationDirection()
    {
      var touch = Input.GetTouch(0);
      
      if (touch.position.x < _halfWidth)
        return Vector3.zero;
      
      if ((touch.deltaPosition).magnitude < _settings.DeltaPosition.magnitude) 
        return Vector3.zero;

      switch (touch.phase)
      {
        case TouchPhase.Moved:
          return new Vector3(-touch.deltaPosition.x,0, touch.deltaPosition.y).normalized;
        case TouchPhase.Stationary:
          return Vector3.zero;
      }
      return Vector3.zero;
    }
  }
}