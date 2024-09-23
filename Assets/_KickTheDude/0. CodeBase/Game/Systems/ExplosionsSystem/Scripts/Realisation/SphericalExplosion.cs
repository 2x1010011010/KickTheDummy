using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Zenject;

public class SphericalExplosion : Explodeable
{
    public override string Name => "SPHERICAL EXPLOSION";

    [Inject]
    public SphericalExplosion(IStaticDataService staticDataService)
    {
        Debug.Log("TEST " + staticDataService);
    }

    protected override ExplosionContact[] GetExplosionContactsWhenExplode()
    {
        return GenerateContacts(GetColliders());
    }

    private List<Collider> GetColliders()
    {
        var colliders = Physics.OverlapSphere(
            _explodePosition.position,
            _explosionParameters.ExplodeRadius,
            _explosionParameters.ExplodeLayerMask,
            QueryTriggerInteraction.Ignore
        ).ToList();

        colliders.RemoveAll(x => x.attachedRigidbody == null);

        return colliders;
    }

    private ExplosionContact[] GenerateContacts(List<Collider> colliders)
    {
        var explosionContacts = new ExplosionContact[colliders.Count];

        for (int i = 0; i < colliders.Count; i++)
        {
            var contactPoint = colliders[i].ClosestPoint(_explodePosition.position);
            explosionContacts[i] = new ExplosionContact(
                colliders[i].attachedRigidbody,
                colliders[i],
                contactPoint,
                _explodePosition.position - contactPoint,
                _explodePosition.position,
                Vector3.Distance(contactPoint, _explodePosition.position)
            );
        }

        return explosionContacts;
    }
}
