using System.Collections.Generic;
using CharacterScripts;
using RootMotion.Dynamics;
using UnityEngine;

namespace Tools.Weapon.Throwable
{
  public class Blow : MonoBehaviour
  {
    [SerializeField] private float _blowDelay = 4f;
    [SerializeField] private float _destroyDelay = 1.5f;
    [SerializeField] private float _force = 1500f;
    [SerializeField] private float _explosionRadius = 5f;
    
    
    private void Start()
    {
      Invoke(nameof(BlowUp), _blowDelay);
    }
    private void BlowUp()
    {
      Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
      foreach (Collider collider in colliders)
      {
        Rigidbody rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
          rb.AddExplosionForce(_force, transform.position, _explosionRadius);
          var bodyPart = rb.GetComponent<BodyPart>();
          if (bodyPart == null) continue;
          bodyPart.TakeDamage();
          var broadcaster = rb.GetComponent<MuscleCollisionBroadcaster>();
          broadcaster?.Hit(100, rb.position.normalized * _force, rb.position);
        }
        
      }

      Invoke(nameof(DestroyObject), 1.0f);
    }
    
    private void DestroyObject()
    {
      Destroy(gameObject);
    }
  }
}