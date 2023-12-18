using UnityEngine;

namespace Infrastructure.Services.InputService
{
  public class MobileInputService : InputService
  {
    public override Vector2 MoveAxis { get; }
    public override Vector2 SpinAxis { get; }

    public override bool IsActionButtonPressed()
    {
      throw new System.NotImplementedException();
    }
  }
}