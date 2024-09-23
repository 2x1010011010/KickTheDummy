using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICroucheable
{
    event Action<bool> CrouchChanged;

    bool CrouchBlocked { get; set; }
    void Crouch(bool crouch);
    void SwitchCrouch();
}
