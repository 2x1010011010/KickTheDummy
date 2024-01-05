using CharacterScripts;
using UnityEngine;
using RootMotion.Dynamics;

namespace Tools.Weapon.Gun
{
  public class Rifle : Gun
  {
    public override void Action()
    {
      if (!CanShoot) return;
      base.Action();
      GameObject blood = null;
      CanShoot = false;
      ElapsedTime = 0;
      

      if (Hit.collider.TryGetComponent(out BodyPart bodyPart))
      {
        var broadcaster = Hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
        broadcaster?.Hit(Unpin, Ray.direction * Settings.Force, Hit.point);
        var index = Random.Range(0, Settings.BloodPrefab.Count);
        var bloodRotation = Quaternion.LookRotation(Ray.direction) * Quaternion.Euler(0, 90, 0);
         blood = Instantiate(Settings.BloodPrefab[index], Hit.point, bloodRotation);
         var bleeding = Instantiate(Settings.Bleeding, Hit.point, bloodRotation);
         bleeding.transform.SetParent(bodyPart.transform);
         bodyPart.TakeDamage();
      }
      
      if(blood != null)
        DestroyBlood(blood);
    }
  }
}