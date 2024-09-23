using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SimplePiercer : IInteractive<IInteractable>, IPiercer
{
    public string Name => "Simple Piercer";

    public event Action<PierceData> Pierce;
    public event Action<IPiercer, IPirceable> Unpierce;

    [SerializeField, BoxGroup("SETUP")] private Rigidbody _selfRigidbody;
    [SerializeField, BoxGroup("SETUP")] private CollisionObserver _collisionObserver;
    [SerializeField, BoxGroup("SETUP")] private CollidersContainer _collidersContainer;

    [OdinSerialize, FoldoutGroup("SHOT REACTORS"), ListDrawerSettings(DraggableItems = false, Expanded = true, ListElementLabelName = "Name")]
    public List<IInteractorAction<IPiercer>> Reactors { get; private set; } = new List<IInteractorAction<IPiercer>>();

    public IInteractable Interactable { get; private set; }
    public IPirceable PiercedObject { get; private set; }

    public void Init(IInteractable initData)
    {
        Interactable = initData;

        _collisionObserver.CollisionEnter += CollisionEnterReact;

        foreach (var reactor in Reactors)
            reactor.Init(this);
    }

    public void Dispose()
    {
        _collisionObserver.CollisionEnter -= CollisionEnterReact;

        foreach (var reactor in Reactors)
            reactor.Dispose();
    }

    private void CollisionEnterReact(Collision collision)
    {
        if(collision.collider.TryGetComponent(out IPirceable pirceable))
        {
            ProcessPierce(collision, pirceable);
        }
    }

    public void StopInteract()
    {
        if(PiercedObject != null) UnpierceFrom(PiercedObject);
    }

    private void ProcessPierce(Collision collision, IPirceable pirceable)
    {
        PierceTo(pirceable);

        var contactPoint = collision.GetContact(0);

        Pierce?.Invoke(new PierceData(contactPoint.point, -contactPoint.normal, collision.collider, this, pirceable));
    }

    public void PierceTo(IPirceable pirceable)
    {
        _selfRigidbody.transform.parent = pirceable.PierceParent;

        _selfRigidbody.detectCollisions = false;
        _selfRigidbody.isKinematic = true;
        _selfRigidbody.interpolation = RigidbodyInterpolation.None;

        _collidersContainer.DeactivateAllColliders();

        pirceable.AddPiercer(this);
    }

    public void UnpierceFrom(IPirceable pirceable)
    {
        _selfRigidbody.transform.parent = null;

        _selfRigidbody.detectCollisions = true;
        _selfRigidbody.isKinematic = false;

        _collidersContainer.ActivateAllColliders();

        Unpierce?.Invoke(this, pirceable);
    }
}
