using UnityEngine;
using System;

public interface IReloadeable
{
    event Action<IReloadeable> Reloaded;

    void Reload();
}