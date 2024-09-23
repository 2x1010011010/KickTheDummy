using System;
using UnityEngine;

public interface ICollisionStay
{
    event Action<Collision, ICollisionEventsProvider> CollisionStay;
}
