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
      
    }

    private void Rotate()
    {
      
    }
  }
}
