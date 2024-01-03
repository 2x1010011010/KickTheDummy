using UnityEngine;
using UnityEngine.Events;

namespace CharacterScripts
{
  public class BodyPart : MonoBehaviour
  {
    [SerializeField] protected BodyPartSettings Settings;
    [SerializeField] protected Rigidbody _rigidbody;

    public Vector3 Velocity => _rigidbody.velocity;
    public event UnityAction<float> OnDamageTaken; 

    public virtual void TakeDamage()
    {
      OnDamageTaken?.Invoke(Random.Range(Settings.MinDamage, Settings.MaxDamage));
    }
  }
}