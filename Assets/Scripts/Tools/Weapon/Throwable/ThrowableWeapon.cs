using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Throwable
{
  public class ThrowableWeapon : MonoBehaviour, IWeapon
  {
    [SerializeField] protected ThrowableSettings _settings;
    [SerializeField] private Vector3 _spawnOffset;
    public virtual void Action()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      var spawned = Instantiate(_settings.Prefab, transform.position + transform.rotation * _spawnOffset, transform.rotation);
      spawned.GetComponent<Rigidbody>().AddForce(ray.direction * _settings.Force, ForceMode.Impulse);
    }
  }
}