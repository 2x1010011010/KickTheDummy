using System;
using UnityEngine;

public interface ICollisionExit
{
    event Action<Collision, ICollisionExit> CollisionExit;
}
