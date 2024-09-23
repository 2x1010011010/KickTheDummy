using System;
using UnityEngine;

public class CollisionSender : MonoBehaviour
{
    public event Action<Collision, CollisionSender> CollisionEnter;
    public event Action<Collision, CollisionSender> CollisionExit;

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision, this);
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionExit?.Invoke(collision, this);
    }
}
