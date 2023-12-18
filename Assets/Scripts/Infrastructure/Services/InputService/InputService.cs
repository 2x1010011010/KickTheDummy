using UnityEngine;

namespace Infrastructure.Services.InputService
{
  public abstract class InputService : IInputService
  {
    protected const string Horizontal = "Horizontal";
    protected const string Vertical = "Vertical";
    protected const string ActionButton = "Fire";
    protected const string MouseHorizontal = "Mouse X";
    protected const string MouseVertical = "Mouse Y";

    public abstract Vector2 MoveAxis { get; }
    public abstract Vector2 SpinAxis { get; }
    public abstract bool IsActionButtonPressed();
  }
}
