using Game.InteractiveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPoint
{
    public readonly InteractableObject InteractableObject;
    public readonly Rigidbody Rigidbody;
    public readonly Collider Collider;
    public readonly Vector3 LocalConnectPoint;

    public Vector3 WorldConnectPoint => Rigidbody.transform.TransformPoint(LocalConnectPoint);

    public ConnectPoint(InteractableObject interactableObject, Rigidbody rigidbody, Collider collider, Vector3 localConnectPoint)
    {
        InteractableObject = interactableObject;
        Rigidbody = rigidbody;
        Collider = collider;
        LocalConnectPoint = localConnectPoint;
    }
}