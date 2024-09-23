using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractorAction<T> : IDisposable
{
    string Name { get; }

    void Init(T reactSource);
}
