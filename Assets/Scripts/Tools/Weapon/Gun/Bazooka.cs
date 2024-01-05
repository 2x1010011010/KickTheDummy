using CharacterScripts;
using RootMotion.Dynamics;
using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Bazooka : Gun
  {
    public override void Action()
    {
      GameObject blood = null;
      base.Action();

      if (!IsHit) return;

      if (Hit.collider.TryGetComponent(out BodyPart bodyPart))
      {
        var broadcaster = Hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
        broadcaster?.Hit(Unpin, Ray.direction * Settings.Force, Hit.point);
        var bloodRotation = Quaternion.LookRotation(Ray.direction);
        blood = Instantiate(Settings.BloodPrefab[0], Hit.point, bloodRotation);
        var bleeding = Instantiate(Settings.Bleeding, Hit.point, bloodRotation);
        bleeding.transform.SetParent(bodyPart.transform);
        bodyPart.TakeDamage();
      }
      if(blood!= null)
        DestroyBlood(blood);
    }
  }
}