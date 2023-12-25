using UnityEngine;

namespace Infrastructure.Services.InputService
{
  public interface IInputService
  {
    Vector2 MoveAxis { get; }
    Vector2 SpinAxis { get; }
  }
}
