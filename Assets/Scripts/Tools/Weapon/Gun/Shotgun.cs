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
      var ray = camera.ScreenPointToRay(Input.mousePosition);
      rays.Add(ray);
      var hits = new List<RaycastHit>();
      
      for (var i = 0; i < _settings.ProjectileAmount; i++)
      {
        var randomDirection = camera.ScreenPointToRay(Input.mousePosition).direction * Random.Range(-0.2f, 0.2f);
        var origin = camera.ScreenToWorldPoint(Input.mousePosition);
        var randomRay = new Ray(origin, new Vector3(randomDirection.x, randomDirection.y, Ray.direction.z));
        rays.Add(randomRay);
        Physics.Raycast(rays[i], out var hit, 100f);
        hits.Add(hit);
      }
      
      
      _isHit = Physics.Raycast(rays[0], out _hit, 100f);
      if (!_isHit) return;

      for (var i = 0; i < _settings.ProjectileAmount; i++)
      {
        if (hits[i].collider.TryGetComponent(out BodyPart bodyPart)) ;
        {
          var index = Random.Range(0, _settings.BloodPrefab.Count);
          var bloodRotation = Quaternion.LookRotation(Ray.direction) * Quaternion.Euler(0, 90, 0);
          var blood = Instantiate(_settings.BloodPrefab[index], _hit.point, bloodRotation);
          bloods.Add(blood);
        }
      }

      foreach(var blood in bloods)
        DestroyBlood(blood);
    }
  }
}