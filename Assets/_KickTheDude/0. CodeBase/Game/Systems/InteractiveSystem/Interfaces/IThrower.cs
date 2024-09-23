using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThrower
{
    void Throw(Action afterThrowAction = null);
}
