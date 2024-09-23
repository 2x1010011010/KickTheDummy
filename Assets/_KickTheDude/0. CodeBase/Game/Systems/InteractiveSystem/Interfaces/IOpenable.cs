using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOpenable
{
    bool IsOpen { get; }
    bool Blocked { get; }

    void Open();
    void Close();
}
