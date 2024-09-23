using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DamageSystem
{
    public class DamageApplyer : MonoBehaviour
    {
        [SerializeField, BoxGroup("SETUP")] private HealthContainer _healthContainer;
        [SerializeField, BoxGroup("SETUP")] private List<DamageSender> _damageSenders = new List<DamageSender>();

        private void OnValidate()
        {
            if (_healthContainer == null) _healthContainer = GetComponent<HealthContainer>();
        }

        private void OnEnable()
        {
            //foreach(var damageSender in _damageSenders)
                //damageSender.OnDamageTaked += OnDamageTaked;
        }

        private void OnDisable()
        {
            //foreach (var damageSender in _damageSenders)
                //damageSender.OnDamageTaked -= OnDamageTaked;
        }

        private void OnDamageTaked(Damage damage, DamageSender damageSender)
        {
            _healthContainer.DecreaseHealthCount((int)damage.DamageCount);
        }
    }
}
