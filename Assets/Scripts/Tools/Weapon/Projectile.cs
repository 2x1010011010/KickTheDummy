using System.Collections.Generic;
using Character;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

namespace Tools.Weapon
{
  [RequireComponent(typeof(Rigidbody))]
  public class Projectile : MonoBehaviour
  {
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private List<GameObject> _bloodEffects;
    
    public void AddForce(float force)
    {
      _rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider collider)
    {
      if (collider == null)
        return;
      
      if (collider.TryGetComponent(out BodyPart bodyPart))
      {
        _rigidbody.velocity = new Vector3(0,0,0);
        _bloodEffects[Random.Range(0, _bloodEffects.Count)].SetActive(true);
        bodyPart.TakeDamage();
      }
    }
  }
}