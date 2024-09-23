using UnityEngine;

public enum ExplodePositionType
{
    Land,
    Air
}

public struct ExplosionContact
{
    public ExplosionContact(Rigidbody rigidbody, Collider collider, Vector3 contactPosition, Vector3 contactNormal, Vector3 explosionPosition, float distanceToExplosionCenter)
    {
        Rigidbody = rigidbody;
        Collider = collider;
        ContactPosition = contactPosition;
        ContactNormal = contactNormal;
        ExplosionCenter = explosionPosition;
        DistanceToExplosionCenter = distanceToExplosionCenter;
    }

    public readonly Rigidbody Rigidbody;
    public readonly Collider Collider;
    public readonly Vector3 ContactPosition;
    public readonly Vector3 ContactNormal;
    public readonly Vector3 ExplosionCenter;
    public readonly float DistanceToExplosionCenter;
}

public struct ExplosionData
{
    public ExplosionData(Vector3 explosionPosition, float explosionRadius, ExplosionContact[] explosionContacts, ExplodePositionType explodePositionType)
    {
        ExplosionPosition = explosionPosition;
        ExplosionRadius = explosionRadius;
        ExplosionContacts = explosionContacts;
        ExplodePositionType = explodePositionType;
    }

    public readonly Vector3 ExplosionPosition;
    public readonly float ExplosionRadius;
    public readonly ExplosionContact[] ExplosionContacts;
    public readonly ExplodePositionType ExplodePositionType;
}
