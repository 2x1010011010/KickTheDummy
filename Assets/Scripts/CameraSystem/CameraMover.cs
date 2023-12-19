using Infrastructure.Services.InputService;
using UnityEngine;
using Zenject;

namespace CameraSystem
{
  public class CameraMover : MonoBehaviour
  {
    [Inject] private CameraSettings _cameraSettings;
    private IInputService _inputService;
    private float _speed;
    private float _sensitivity;
    private Vector3 _newPosition;

    [Inject]
    private void Construct(IInputService inputService)
    {
      _inputService = inputService;
      _speed = _cameraSettings.MovementSpeed;
      _sensitivity = _cameraSettings.RotationSpeed;
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
