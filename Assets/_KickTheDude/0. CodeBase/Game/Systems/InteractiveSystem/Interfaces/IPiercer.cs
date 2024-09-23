using System;
using UnityEngine;

public struct PierceData
{
    public Vector3 PiercePoint;
    public Vector3 PierceDirection;
    public Collider PiercedCollider;
    public IPiercer Piercer;
    public IPirceable Pierceable;

    public PierceData(Vector3 piercePoint, Vector3 pierceDirection, Collider pierceCollider, IPiercer piercer, IPirceable pirceable)
    {
        PiercePoint = piercePoint;
        PierceDirection = pierceDirection;
        PiercedCollider = pierceCollider;
        Piercer = piercer;
        Pierceable = pirceable;
    }
}

public interface IPiercer
{
    event Action<PierceData> Pierce;
    event Action<IPiercer, IPirceable> Unpierce;

    IPirceable PiercedObject { get; }

    void PierceTo(IPirceable pirceable);
    void UnpierceFrom(IPirceable pirceable);
}
