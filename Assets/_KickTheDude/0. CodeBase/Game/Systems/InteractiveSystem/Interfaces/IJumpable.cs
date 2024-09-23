using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJumpable
{
    event Action Jumped;

    void Jump();
}
