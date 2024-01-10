using System.Collections.Generic;
using System.Linq;
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
    PuppetMaster _puppet;
    
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
          _puppet = bodyPart.GetComponentInParent<PuppetMaster>();
          var broadcaster = rb.GetComponent<MuscleCollisionBroadcaster>();
          broadcaster?.Hit(0, rb.position.normalized * _force, rb.position);
        }
      }

      Invoke(nameof(DestroyObject), 1.0f);
      if (_puppet == null) return;
      for (int i = 0; i < _puppet.muscles.Length / 2; i++)
      {
        var index = Random.Range(i, _puppet.muscles.Length);
        _puppet.DisconnectMuscleRecursive(index, MuscleDisconnectMode.Explode,false);
      }
    }
    
    private void DestroyObject()
    {
      Destroy(gameObject);
    }
  }
}