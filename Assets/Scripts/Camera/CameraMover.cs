using Infrastructure.Services.InputService;
using UnityEngine;
using Zenject;

public class CameraMover : MonoBehaviour
{
  private IInputService _inputService;
  private float _speed;
  private float _sensitivity;
  private Vector3 _newPosition;

  [Inject]
  private void Construct(IInputService inputService)
  {
    _inputService = inputService;
    _speed = 10f;
    _sensitivity = 10f;
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
