using System;
using UnityEngine;

public interface ICollisionEventsProvider : ICollisionEnter, ICollisionExit
{
    bool IsColliding { get; }
}
