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
      var spawned = Instantiate(_settings.Prefab, transform.position + transform.rotation * _spawnOffset, transform.rotation);
      var rb = spawned.GetComponent<Rigidbody>();

      if (rb == null) return;
      
      rb.mass = _settings.Mass;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      rb.AddForce(Quaternion.LookRotation(ray.direction) * new Vector3(0f,0f,_settings.Force), ForceMode.Impulse);
    }
  }
}