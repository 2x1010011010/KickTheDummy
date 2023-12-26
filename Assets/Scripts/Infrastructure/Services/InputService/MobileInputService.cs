using CameraSystem;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.InputService
{
  public sealed class MobileInputService : InputService
  {
    public override Vector2 MoveAxis => Move();
    public override Vector2 SpinAxis => Rotation();
    private  MobileCameraController _movementController;
    private MobileCameraController _rotationController;


    private Vector2 Move()
    {
      if(!_movementController.OnPressed)
        return Vector2.zero;

      foreach (var touch in Input.touches)
      {
        if (touch.fingerId != _movementController.FingerID) continue;
        if (touch.phase == TouchPhase.Moved)
          return new Vector2(touch.deltaPosition.x, touch.deltaPosition.y).normalized;
      }
      return Vector2.zero;
    }


    private Vector2 Rotation()
    {
      if(!_movementController.OnPressed)
        return Vector2.zero;

      foreach (var touch in Input.touches)
      {
        if (touch.fingerId != _rotationController.FingerID) continue;
        if (touch.phase == TouchPhase.Moved)
          return new Vector2(touch.deltaPosition.x, touch.deltaPosition.y).normalized;
      }
      return Vector2.zero;
    }
  }
}