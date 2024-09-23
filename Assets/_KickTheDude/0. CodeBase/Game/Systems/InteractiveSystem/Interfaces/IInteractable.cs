using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.InteractiveSystem
{
    public interface IInteractable : IDestroyeable
    {
        Transform Root { get; }
        IReadOnlyList<IInteractive<IInteractable>> Interactives { get; }
        IReadOnlyList<IInteractiveAction> InteractiveActions { get; }

        void StopInteract();
        bool HasInteractive<T>();
        T GetInteractive<T>();
        List<T> GetAllInteractivesByType<T>();
        bool TryGetInteractive<T>(out T result);

        Coroutine StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(Coroutine coroutine);
    }
}