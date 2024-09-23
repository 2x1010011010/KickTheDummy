using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InteractableObjectSaveData
{
    public string ResourceID;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public bool Freezed;
    public List<RigidbodyState> RigidbodyStates;
}

[Serializable]
public class RigidbodyState
{
    public Vector3 Position;
    public Vector3 Rotation;
}
