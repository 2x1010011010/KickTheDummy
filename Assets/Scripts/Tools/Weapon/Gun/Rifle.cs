using Tools.Weapon.WeaponSettings;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

namespace Tools.Weapon.Gun
{
  public class Rifle : Gun
  {
    [SerializeField] private GunSettings _settings;
    private float _elapsedTime = 0;

    public override void Action()
    {
      _elapsedTime += Time.deltaTime;
      if (!(_elapsedTime >= _settings.ReloadDelay)) return;
      base.Action();
      GameObject blood;
      if (!_isHit) return;

      if (_hit.collider.TryGetComponent(out Character.BodyPart bodyPart)) ;
      {
        var index = Random.Range(0, _settings.BloodPrefab.Count);
        var bloodRotation = Quaternion.LookRotation(Ray.direction) * Quaternion.Euler(0, 90, 0);
         blood = Instantiate(_settings.BloodPrefab[index], _hit.point, bloodRotation);
      }
      _elapsedTime = 0f;
      DestroyBlood(blood);
    }
  }
}