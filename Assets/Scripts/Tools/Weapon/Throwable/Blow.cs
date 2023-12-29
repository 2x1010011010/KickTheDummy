using UnityEngine;

namespace Tools.Weapon.Throwable
{
  public class Blow : MonoBehaviour
  {
    [SerializeField] private float _blowDelay = 4f;
    [SerializeField] private float _destroyDelay = 1.5f;
    [SerializeField] private float _force = 1500f;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private Rigidbody _rigidbody;
    
    
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
        }
      }
      Invoke(nameof(DestroyObject), _destroyDelay);
    }
    
    private void DestroyObject()
    {
      Destroy(gameObject);
    }
  }
}