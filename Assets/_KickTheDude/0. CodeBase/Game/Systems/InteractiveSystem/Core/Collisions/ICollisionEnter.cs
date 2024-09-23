using System;
using UnityEngine;

public interface ICollisionEnter
{
    event Action<Collision, ICollisionEnter> CollisionEnter;
}
