using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivatiable
{
    bool IsActive { get; }

    void Activate();
    void Deactivate();
}
