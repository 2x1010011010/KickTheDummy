using System;
using UnityEngine;

public class CollisionListener : MonoBehaviour
{
    public event Action<Collision> OnCollisionEnterCaught;
    public event Action<Collision> OnCollisionExitCaught;

    private void OnCollisionEnter(Collision collision)
    {
        NotifyAboudCollisionEnterHappened(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        NotifyAboudCollisionExitHappened(collision);
    }

    private void NotifyAboudCollisionEnterHappened(Collision collision)
        => OnCollisionEnterCaught?.Invoke(collision);

    private void NotifyAboudCollisionExitHappened(Collision collision)
        => OnCollisionExitCaught?.Invoke(collision);
}