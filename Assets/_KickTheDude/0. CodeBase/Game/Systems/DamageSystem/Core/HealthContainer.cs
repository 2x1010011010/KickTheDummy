using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Game.DamageSystem
{
    public class HealthContainer : MonoBehaviour, IRestoreable
    {
        public event Action<HealthContainer, float> OnHealthIncreased;
        public event Action<HealthContainer, float> OnHealthDecreased;
        public event Action<HealthContainer, float> OnHealthEnded;
        public event Action<HealthContainer, float> OnHealthRestored;

        [SerializeField, BoxGroup("SETUP")] private HealthParameters _healthParameters;
        [SerializeField, BoxGroup("DEBUG"), Min(0), ReadOnly] private int _curentHealth;

        public int CurentHealth => _curentHealth;
        public bool IsHealthEnded => _curentHealth <= 0 ? true : false;

        private void OnValidate()
        {
            if (_healthParameters == null) return;

            ClampHealthValue();
        }

        public void IncreaseHealthCount(int amount)
        {
            _curentHealth += amount;
            ClampHealthValue();

            OnHealthIncreased?.Invoke(this, amount);

            if (_curentHealth == _healthParameters.MaxHealth)
                OnHealthRestored?.Invoke(this, _curentHealth);
        }

        public void DecreaseHealthCount(int amount)
        {
            if (_curentHealth <= 0) return;

            _curentHealth -= amount;
            ClampHealthValue();

            OnHealthDecreased?.Invoke(this, amount);

            if (IsHealthEnded)
                OnHealthEnded?.Invoke(this, amount);
        }

        [Button("RESTORE", ButtonSizes.Large), BoxGroup("ACTIONS")]
        public void Restore()
        {
            _curentHealth = _healthParameters.MaxHealth;

            OnHealthRestored?.Invoke(this, _curentHealth);
        }

        private void ClampHealthValue()
        {
            _curentHealth = Mathf.Clamp(_curentHealth, 0, _healthParameters.MaxHealth);
        }

        public void EndHealth()
        {
            _curentHealth = 0;

            OnHealthDecreased?.Invoke(this, 0);
            OnHealthEnded?.Invoke(this, 0);
        }
    }
}