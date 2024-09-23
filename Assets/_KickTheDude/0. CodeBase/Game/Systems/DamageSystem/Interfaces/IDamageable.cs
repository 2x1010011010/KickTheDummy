using System;
using UnityEngine;

public enum DamageType
{
    Punch,
    Hit,
    Pierce,
    Cut,
    None,
    Explosion,
    Shot
}

public struct Damage
{
    public Damage(float damageCount, Transform damageSender, Collider collider, DamageType damageType, Vector3 position, Vector3 normal)
    {
        DamageCount = damageCount;
        DamageSender = damageSender;
        Collider = collider;
        DamageType = damageType;
        Position = position;
        Normal = normal;
    }

    public Damage(float damageCount)
    {
        DamageCount = damageCount;
        DamageSender = null;
        Collider = null;
        DamageType = DamageType.None;
        Position = Vector3.zero;
        Normal = Vector3.zero;
    }

    public readonly float DamageCount;
    public readonly Transform DamageSender;
    public readonly Collider Collider;
    public readonly DamageType DamageType;
    public readonly Vector3 Position;
    public readonly Vector3 Normal;
}

public interface IDamageable
{
    event Action<Damage, IDamageable> DamageTaken;

    void TakeDamage(Damage damage);
}