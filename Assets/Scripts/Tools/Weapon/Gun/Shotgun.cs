using System.Collections.Generic;
using CharacterScripts;
using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Shotgun : Gun
  {
    [SerializeField] private GunSettings _settings;
    
    public override void Action()
    {
      List<GameObject> bloods = new List<GameObject>();
      List<Ray> rays = new List<Ray>();
      var camera = Camera.main;
      var hits = new List<RaycastHit>();

      for (var i = 0; i < _settings.ProjectileAmount; i++)
      {
        RaycastHit hit;

        Vector3 shootDirection = camera.transform.forward;
        

        if (!Physics.Raycast(camera.transform.position, shootDirection, out hit, 100f)) continue;
        
        if (hit.collider.TryGetComponent(out BodyPart bodyPart))
        {
          var index = Random.Range(0, _settings.BloodPrefab.Count);
          var bloodRotation = Quaternion.LookRotation(shootDirection) * Quaternion.Euler(0, 90, 0);
          var blood = Instantiate(_settings.BloodPrefab[index], hit.point, bloodRotation);
          bodyPart.TakeDamage();
          var bleeding = Instantiate(_settings.Bleeding, hit.point, bloodRotation);
          bleeding.transform.SetParent(bodyPart.transform);
          bloods.Add(blood);
        }
      }

      if (bloods != null)
      {
        foreach(var blood in bloods)
          DestroyBlood(blood);
      }
    }
  }
}