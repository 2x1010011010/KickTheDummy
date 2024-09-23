using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    void Move(Vector3 direction, bool local);
    void Move(Vector2 input);
    void StopMoving();
}
