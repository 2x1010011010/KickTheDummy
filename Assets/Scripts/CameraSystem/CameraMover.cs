using Infrastructure.Services.InputService;
using UnityEngine;
using Zenject;

namespace CameraSystem
{
  public sealed class CameraMover : MonoBehaviour
  {
    [Inject] private CameraSettings _settings;
    private IInputService _inputService;
    private float _speed;
    private float _sensitivity;
    private Vector3 _newPosition;
    private float _rotationOffset;

    private Vector3 _centerPoint = new Vector3(0, 0, 0);

    [Inject]
    private void Construct(IInputService inputService)
    {
      _inputService = inputService;
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

    private void Move()
    {
      Vector3 direction = GetMoveDirection();
      _newPosition += direction * (_speed * Time.deltaTime);
      transform.position = _newPosition;
    }
    
    private Vector3 GetMoveDirection() => 
      new Vector3(_inputService.MoveAxis.x, 0, _inputService.MoveAxis.y);
    

    private void Rotate()
    {
     
    }
  }
}