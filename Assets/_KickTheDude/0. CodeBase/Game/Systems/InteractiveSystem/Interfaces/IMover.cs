using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMover
{
    void Move(IMoveable moveable, Vector3 direction);
    void Jump(IMoveable moveable);
    void Crouch(ICroucheable croucheable);
}
