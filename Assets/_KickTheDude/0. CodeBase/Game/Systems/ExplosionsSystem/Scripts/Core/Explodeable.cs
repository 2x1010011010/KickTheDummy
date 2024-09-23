using System;
using UnityEngine;
using System.Collections.Generic;
using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public abstract class Explodeable : IInteractive<IInteractable>, IExplodeable
{
    public abstract string Name { get; }

    public event Action<ExplosionData> Exploded;

    [BoxGroup("SETUP")]
    [SerializeField] protected Transform _explodePosition;

    [BoxGroup("SETUP")]
    [SerializeField] protected ExplosionParameters _explosionParameters;

    [OdinSerialize, FoldoutGroup("REACTORS"), ListDrawerSettings(DraggableItems = false, ShowFoldout = true, ListElementLabelName = "Name")]
    public List<IInteractorAction<IExplodeable>> Reactors { get; private set; } = new List<IInteractorAction<IExplodeable>>();

    public IInteractable Interactable { get; private set; }

    public void Init(IInteractable interactable)
    {
        Interactable = interactable;

        foreach (var reactor in Reactors)
            reactor.Init(this);
    }

    public void Dispose()
    {
        foreach (var reactor in Reactors)
            reactor.Dispose();
    }

    public void StopInteract() {}

    [Button(ButtonSizes.Large), BoxGroup("ACTIONS")]
    public void Explode()
    {
        var explosionContacts = GetExplosionContactsWhenExplode();

        NotifyAboutExplosion(
            new ExplosionData(
                _explodePosition.position,
                _explosionParameters.ExplodeRadius,
                explosionContacts,
                GetExplodePositionType(
                    _explodePosition.position,
                    _explosionParameters.LandLayerMask,
                    _explosionParameters.CheckLandDistance
                )
            )
        );
    }

    protected abstract ExplosionContact[] GetExplosionContactsWhenExplode();

    private ExplodePositionType GetExplodePositionType(Vector3 explodePosition, LayerMask landFindMask, float landFindDistance)
    {
        if (Physics.Raycast(explodePosition, Vector3.down, landFindDistance, landFindMask, QueryTriggerInteraction.Ignore))
        {
            return ExplodePositionType.Land;
        }
        else
        {
            return ExplodePositionType.Air;
        }
    }

    private void NotifyAboutExplosion(ExplosionData explosionData)
    {
        Exploded?.Invoke(explosionData);
    }

    private void OnDrawGizmosSelected()
    {
        if (_explosionParameters == null) return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(_explodePosition.position, 0.1f);
        Gizmos.DrawWireSphere(_explodePosition.position, _explosionParameters.ExplodeRadius);

        Gizmos.DrawRay(_explodePosition.position, _explodePosition.forward * _explosionParameters.ExplodeRadius);
        Gizmos.DrawRay(_explodePosition.position, -_explodePosition.forward * _explosionParameters.ExplodeRadius);
        Gizmos.DrawRay(_explodePosition.position, _explodePosition.right * _explosionParameters.ExplodeRadius);
        Gizmos.DrawRay(_explodePosition.position, -_explodePosition.right * _explosionParameters.ExplodeRadius);
    }
}
