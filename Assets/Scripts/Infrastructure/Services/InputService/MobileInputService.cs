using CameraSystem;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.InputService
{
  public sealed class MobileInputService : InputService
  {
    public override Vector2 MoveAxis => Move();
    public override Vector2 SpinAxis => Rotation();
    

    private Vector2 Move()
    {
      
      return Vector2.zero;
    }


    private Vector2 Rotation()
    {
      
      return Vector2.zero;
    }
  }
}