using System.Collections.Generic;
using CharacterScripts;
using RootMotion.Dynamics;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Shotgun : Gun
  {
    public override void Action()
    {
      List<GameObject> bloods = new List<GameObject>();
      var camera = Camera.main;

      for (var i = 0; i < Settings.ProjectileAmount; i++)
      {
        base.Action();

        if (!Physics.Raycast(Ray,  out Hit, 100f)) continue;
        
        if (Hit.collider.TryGetComponent(out BodyPart bodyPart))
        {
          var broadcaster = Hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
          broadcaster?.Hit(100.0f, Ray.direction * Settings.Force, Hit.point);
          var index = Random.Range(0, Settings.BloodPrefab.Count);
          var bloodRotation = Quaternion.LookRotation(Ray.direction) * Quaternion.Euler(0, 90, 0);
          var blood = Instantiate(Settings.BloodPrefab[index], Hit.point, bloodRotation);
          bodyPart.TakeDamage();
          var bleeding = Instantiate(Settings.Bleeding, Hit.point, bloodRotation);
          bleeding.transform.SetParent(bodyPart.transform);
          bloods.Add(blood);
        }
        
        
      }
      
      
      if (bloods != null)
        foreach(var blood in bloods)
          DestroyBlood(blood);
    }
  }
}