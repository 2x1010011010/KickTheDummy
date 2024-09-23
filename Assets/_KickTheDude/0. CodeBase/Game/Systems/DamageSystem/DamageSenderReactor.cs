using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageSenderReactor : MonoBehaviour
{
    [SerializeField] private DamageSender _damageSender;
    [SerializeField] private UnityEvent DoWhenDamageTaked;

    private void OnValidate()
    {
        _damageSender = GetComponent<DamageSender>();
    }

    private void OnEnable()
    {
        //_damageSender.OnDamageTaked += OnDamageTaked;
    }

    private void OnDisable()
    {
        //_damageSender.OnDamageTaked -= OnDamageTaked;
    }

    private void OnDamageTaked(Damage damage, DamageSender damageSender)
    {
        DoWhenDamageTaked?.Invoke();
    }
}
