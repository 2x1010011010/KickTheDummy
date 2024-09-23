using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DamageSystem
{
    public class DamageSystem : MonoBehaviour
    {
        public event Action<Damage> DamageTaked;

        [SerializeField] private HealthContainer _healthContainer;
        [SerializeField] private List<DamageSender> _damageSenders;

        [SerializeField, ReadOnly] private Transform _lastDamageSender;

        public Transform LastDamageSender { get { return _lastDamageSender; } }

        private void OnValidate()
        {
            if (_healthContainer == null) _healthContainer = GetComponent<HealthContainer>();
        }

        private void OnEnable()
        {
            //foreach(var damageSender in _damageSenders)
                //damageSender.OnDamageTaked += DamageTakedReact;
        }

        private void OnDisable()
        {
            //foreach (var damageSender in _damageSenders)
                //damageSender.OnDamageTaked -= DamageTakedReact;
        }

        private void DamageTakedReact(Damage damage, DamageSender damageSender)
        {
            _healthContainer.DecreaseHealthCount((int)damage.DamageCount);

            _lastDamageSender = damage.DamageSender;

            DamageTaked?.Invoke(damage);
        }
    }
}
