using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalRollMovement : IInteractive<IInteractable>, IMoveable, IJumpable
{
    public string Name => "ROLL MOVEMENT";

    public event Action Jumped;

    [SerializeField, BoxGroup("SETUP")] private Rigidbody _moveableRigidbody;
    [SerializeField, BoxGroup("SETUP")] private ForceParameters _moveForceParameters;
    [SerializeField, BoxGroup("SETUP")] private ForceParameters _moveInAirForceParameters;
    [SerializeField, BoxGroup("SETUP")] private ForceParameters _jumpParameters;
    [SerializeField, BoxGroup("SETUP")] private ICollisionEventsProvider _collisionSender;

    public IInteractable Interactable { get; private set; }

    public void Init(IInteractable initData)
    {
        Interactable = initData;
    }

    public void Dispose()
    {

    }

    public void StopInteract()
    {

    }

    public void Move(Vector3 direction, bool local)
    {
        if(_collisionSender.IsColliding)
            _moveableRigidbody.AddForce(direction * _moveForceParameters.Force, _moveForceParameters.ForceMode);
        else
            _moveableRigidbody.AddForce(direction * _moveInAirForceParameters.Force, _moveInAirForceParameters.ForceMode);
    }

    public void Jump()
    {
        if(_collisionSender.IsColliding)
            _moveableRigidbody.AddForce(Vector3.up * _jumpParameters.Force, _jumpParameters.ForceMode);
    }

    public void MoveToPoint(Vector3 point)
    {

    }

    public void StopMoving()
    {
        _moveableRigidbody.velocity = Vector3.zero;
    }

    public void Move(Vector2 input)
    {
        throw new System.NotImplementedException();
    }
}
