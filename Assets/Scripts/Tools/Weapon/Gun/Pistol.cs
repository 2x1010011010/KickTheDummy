using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Pistol : Gun
  {
    [SerializeField] private GunSettings _settings;
    
    public override void Action()
    {
      var spawned = Instantiate(_settings.ProjectilePrefab, Camera.main.transform.position, Quaternion.identity);
      spawned.GetComponent<Projectile>().AddForce(_settings.Force);
    }
  }
}