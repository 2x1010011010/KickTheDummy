using System.Collections.Generic;
using UnityEngine;

namespace CharacterScripts
{
  public class CharacterDamageController : MonoBehaviour
  {
    [SerializeField] private List<BodyPart> _body;
    [SerializeField] private float _maxHealth = 100f;
    
    private float _currentHealth;
    private void Start()
    {
      _currentHealth = _maxHealth;
    }
    
    private void TakeDamage(float damage)
    {
      _currentHealth -= damage;
      if (_currentHealth <= 0)
      {
        Die();
      }
    }
    private void Die()
    {
      // Handle character death
    }
  }
}