using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Throwable
{
  public class ThrowableWeapon : MonoBehaviour, IWeapon
  {
    [SerializeField] private ThrowableSettings _settings;
    [SerializeField] private Vector3 _spawnOffset;
    public virtual void Action()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      var spawned = Instantiate(_settings.Prefab, transform.position + transform.rotation * _spawnOffset, transform.rotation);
      var rb = spawned.GetComponent<Rigidbody>();

      if (rb == null) return;
      
      rb.mass = _settings.Mass;
      rb.AddForce(Quaternion.LookRotation(ray.direction) * (Vector3.forward * _settings.Force), ForceMode.Impulse);
    }
  }
}