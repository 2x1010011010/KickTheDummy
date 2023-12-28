using Tools.Weapon.WeaponSettings;
using UnityEditor;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Pistol : Gun
  {
    [SerializeField] private GunSettings _settings;
    
    public override void Action()
    {
      GameObject blood;
      base.Action();

      if (!_isHit) return;
     
      if (_hit.collider.TryGetComponent(out Character.BodyPart bodyPart)) ;
      {
        var index = Random.Range(0, _settings.BloodPrefab.Count);
        var bloodRotation = Quaternion.LookRotation(Ray.direction) * Quaternion.Euler(0,90,0);
        blood = Instantiate(_settings.BloodPrefab[index], _hit.point, bloodRotation);
        //blood.Emit(5);
      }
      DestroyBlood(blood);
    }
  }
}