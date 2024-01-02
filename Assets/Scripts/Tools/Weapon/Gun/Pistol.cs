using CharacterScripts;
using RootMotion.Dynamics;
using Tools.Weapon.WeaponSettings;
using UnityEngine;


namespace Tools.Weapon.Gun
{
  public class Pistol : Gun
  {
    [SerializeField] private GunSettings _settings;
    
    public override void Action()
    {
      GameObject blood = null;
      base.Action();

      if (!_isHit) return;
     
      if (_hit.collider.TryGetComponent(out BodyPart bodyPart))
      {
        var broadcaster = _hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
        broadcaster?.Hit(_unpin, Ray.direction * _settings.Force, _hit.point);
        var index = Random.Range(0, _settings.BloodPrefab.Count);
        var bloodRotation = Quaternion.LookRotation(Ray.direction) * Quaternion.Euler(0,90,0);
        blood = Instantiate(_settings.BloodPrefab[index], _hit.point, bloodRotation);
        bodyPart.TakeDamage();
      }
      DestroyBlood(blood);
    }
  }
}