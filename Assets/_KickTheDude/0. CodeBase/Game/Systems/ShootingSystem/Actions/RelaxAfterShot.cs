using RootMotion.Dynamics;
using UnityEngine;

public class RelaxAfterShot : ShotAction
{
    public override string Name => "RELAX RAGDOLL";

    [SerializeField] private RelaxParameters _relaxParameters;

    public override void ReactOnShot(ShotData shotData)
    {
        if (shotData.ShotCollider == null) return;
        if (shotData.ShotCollider.attachedRigidbody == null) return;

        if (shotData.ShotCollider.attachedRigidbody.TryGetComponent(out MuscleCollisionBroadcaster relaxable))
        {
            relaxable.Hit(_relaxParameters.Unpin, shotData.Direction * _relaxParameters.Force, shotData.ShotPoint);
        }
    }
}
