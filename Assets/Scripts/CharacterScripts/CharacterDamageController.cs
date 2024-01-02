using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

namespace CharacterScripts
{
  public class CharacterDamageController : MonoBehaviour
  {
    [SerializeField] private List<BodyPart> _body;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private PuppetMaster _puppetMaster; 
    [SerializeField] private Vector3 _disconnectVelocity;
    
    private float _currentHealth;

    private void Start()
    {
      _currentHealth = _maxHealth;
    }

    private void OnEnable()
    {
      foreach (var item in _body)
        item.OnDamageTaken += TakeDamage;
    }

    private void OnDisable()
    {
      foreach (var item in _body)
        item.OnDamageTaken -= TakeDamage;
    }

    private void TakeDamage(float damage)
    {
      _currentHealth -= damage;
      _puppetMaster.pinWeight -= damage / _maxHealth;
      
      if (_currentHealth <= 0)
      {
        _puppetMaster.Kill();
      }

      for (var i = 0; i < _body.Count; i++)
      {
        if (_body[i].Velocity.x >= _disconnectVelocity.x || 
            _body[i].Velocity.y >= _disconnectVelocity.y ||
            _body[i].Velocity.z >= _disconnectVelocity.z)
        {
          _puppetMaster.DisconnectMuscleRecursive(i);
          _body[i].OnDamageTaken -= TakeDamage;
        }
      }
    }
  }
}