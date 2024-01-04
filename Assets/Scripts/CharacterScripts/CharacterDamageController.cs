using System.Collections.Generic;
using System.Linq;
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
    private bool _isBleeding = false;
    private int _bleedingCount;
    private const float BleedingDamage = 1f;
    private HashSet<BodyPart> _bleedingParts = new HashSet<BodyPart>();
    private float _elapsedTime;
    private bool _isDead = false;

    private void Start()
    {
      _currentHealth = _maxHealth;
      _bleedingCount = 0;
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

    private void Update()
    {
      if (_isDead) return;
      _elapsedTime += Time.deltaTime;
      if (_isBleeding && _elapsedTime >= 2f)
      {
        TakeDamage(BleedingDamage * _bleedingCount);
        _elapsedTime = 0;
      }
    }

    private void TakeDamage(float damage)
    {
      _currentHealth -= damage;
      _puppetMaster.pinWeight -= damage / _maxHealth;

      foreach (var part in _body.Where(part => part.GetComponentInChildren<Bleeding>()).Where(part => !_bleedingParts.Contains(part)))
      {
        _bleedingParts.Add(part);
        _isBleeding = true;
        _bleedingCount++;
      }

      if (_currentHealth <= 0)
      {
        _puppetMaster.Kill();
        _isDead = true;
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