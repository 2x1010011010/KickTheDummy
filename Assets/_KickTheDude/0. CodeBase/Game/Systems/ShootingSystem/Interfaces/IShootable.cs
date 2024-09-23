using System;
using System.Collections.Generic;
using UnityEngine;

public struct ShotData
{
    public Vector3 InitialPosition;
    public Vector3 Direction;

    public Vector3 ShotPoint;
    public Vector3 ShotNormal;
    public Collider ShotCollider;
    public IShootable Shootable;

    public RaycastHit RaycastHit;

    public ShotData(RaycastHit raycastHit, Vector3 position, Vector3 direction, Vector3 shotPoint, Vector3 shotNormal, Collider shotCollider, IShootable shootable)
    {
        RaycastHit = raycastHit;
        InitialPosition = position;
        Direction = direction;
        ShotPoint = shotPoint;
        ShotNormal = shotNormal;
        ShotCollider = shotCollider;
        Shootable = shootable;
    }
}

public interface IShootable : IRestoreable
{
    event Action<ShotData> Shoted;

    List<IInteractorAction<IShootable>> Reactors { get; }

    void Shot(Vector3 position, Vector3 direction);
    void StopShooting();
}

