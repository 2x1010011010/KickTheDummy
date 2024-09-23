using Game.InteractiveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionState
{
    Allowed,
    Blocked,
    Hidden
}

public interface IInteractiveAction : IInitialble, IDisposable
{
    ActionState CurrentState { get; }
    string ActionName { get; }

    void DoAction();
}
