using UnityEngine;

namespace Infrastructure.Services.InputService
{
  public sealed class MobileInputService : InputService
  {
    public override Vector2 MoveAxis => Move();
    public override Vector2 SpinAxis => Rotation();

    private Vector2 Move()
    {
      return new Vector2(0,0);
    }

    private Vector2 Rotation()
    {
      return new Vector2(0, 0);
    }
  }
}