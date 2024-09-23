using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive<IInteractable> : IInitiable<IInteractable>, IDisposable
{
    string Name { get; }

    IInteractable Interactable { get; }

    void StopInteract();
}
