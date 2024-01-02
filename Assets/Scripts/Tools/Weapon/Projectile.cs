using System;
using System.Collections.Generic;
using CharacterScripts;
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
    [SerializeField] private float _destroyAfterCollisionDelay = 0.5f;
    [SerializeField] private float _destroyDelay = 3f;
    private float _elapsedTime = 0;
    private bool _isMoved = false;
    private Ray _ray;
    private Sequence _sequence;

    public void AddForce(Ray ray, Vector3 force)
    {
      _elapsedTime = 0;
      _isMoved = true;
      _rigidbody.AddForce(Quaternion.LookRotation(ray.direction) * force, ForceMode.Impulse);
      _ray = ray;
    }

    private void OnTriggerEnter(Collider collider)
    {
      if (collider == null)
        return;
      _rigidbody.velocity = new Vector3(0,0,0);
      _rigidbody.isKinematic = true;
      if (collider.TryGetComponent(out BodyPart bodyPart))
      {
        _rigidbody.velocity = new Vector3(0,0,0);
        _rigidbody.isKinematic = true;

        var index = Random.Range(0, _bloodEffects.Count);
        _bloodEffects[index].transform.rotation = Quaternion.LookRotation(-_ray.direction);
        _bloodEffects[index].SetActive(true);
        
        //bodyPart.TakeDamage();
      }
      _sequence = DOTween.Sequence();
      _sequence.AppendInterval(_destroyAfterCollisionDelay);
      _sequence.AppendCallback(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
      if(_sequence!=null)
        DOTween.Kill(_sequence);
    }
  }
}