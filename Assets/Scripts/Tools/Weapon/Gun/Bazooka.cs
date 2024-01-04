using CharacterScripts;
using RootMotion.Dynamics;
using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Bazooka : Gun
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
        var bloodRotation = Quaternion.LookRotation(Ray.direction);
        blood = Instantiate(_settings.BloodPrefab[0], _hit.point, bloodRotation);
        var bleeding = Instantiate(_settings.Bleeding, _hit.point, bloodRotation);
        bleeding.transform.SetParent(bodyPart.transform);
        bodyPart.TakeDamage();
      }
      if(blood!= null)
        DestroyBlood(blood);
    }
  }
}