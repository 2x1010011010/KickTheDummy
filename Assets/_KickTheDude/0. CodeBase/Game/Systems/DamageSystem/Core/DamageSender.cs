using System;
using UnityEngine;

public class DamageSender : MonoBehaviour, IDamageable
{
    public event Action<Damage, IDamageable> DamageTaken;

    public void TakeDamage(Damage damage)
    {
        DamageTaken?.Invoke(damage, this);
    }
}