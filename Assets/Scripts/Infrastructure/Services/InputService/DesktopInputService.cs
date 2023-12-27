using PaintIn3D;
using UnityEngine;

namespace Infrastructure.Services.InputService
{
  public sealed class DesktopInputService : InputService
  {
    public override Vector2 MoveAxis => ButtonsAxis();

    public override Vector2 SpinAxis => MouseAxis();

    private Vector2 ButtonsAxis() =>
      new Vector2(Input.GetAxis(Horizontal), Input.GetAxis(Vertical));

    private Vector2 MouseAxis() =>
      new Vector2(Input.GetAxis(MouseHorizontal), Input.GetAxis(MouseVertical));
  }
}