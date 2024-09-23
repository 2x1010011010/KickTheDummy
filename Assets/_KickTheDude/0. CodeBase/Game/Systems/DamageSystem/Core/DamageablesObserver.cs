using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageablesObserver : MonoBehaviour
{
    public event Action<Damage, DamageSender> OnDamageTaked;

    [SerializeField] private Transform _damageSendersRoot;
    [SerializeField] private DamageSender[] _damageSenders;

    private void OnEnable()
    {
        //foreach(DamageSender damageSender in _damageSenders)
            //damageSender.OnDamageTaked += OnDamageTakedReact;
    }

    private void OnDisable()
    {
        //foreach (DamageSender damageSender in _damageSenders)
            //damageSender.OnDamageTaked -= OnDamageTakedReact;
    }

    private void OnDamageTakedReact(Damage damage, DamageSender damageSender)
    {
        OnDamageTaked?.Invoke(damage, damageSender);
    }

    [ContextMenu("FindDamageSenders")]
    private void FindDamageSenders()
    {
        _damageSenders = _damageSendersRoot.GetComponentsInChildren<DamageSender>();
    }
}
