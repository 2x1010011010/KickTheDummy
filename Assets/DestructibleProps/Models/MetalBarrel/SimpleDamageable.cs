using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDamageable : SerializedMonoBehaviour
{
    [SerializeField] private List<DamageSender> _damageSenders = new List<DamageSender>();

    private void OnEnable()
    {
        foreach (var damageSender in _damageSenders)
            damageSender.DamageTaken += DamageTaken;
    }

    private void OnDisable()
    {
        foreach (var damageSender in _damageSenders)
            damageSender.DamageTaken -= DamageTaken;
    }

    protected virtual void DamageTaken(Damage damage, IDamageable damageable) { }
}
