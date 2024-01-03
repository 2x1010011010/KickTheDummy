using CharacterScripts;
using RootMotion.Dynamics;
using UnityEngine;

namespace Tools.Weapon.Melee
{
  public class Hammer : MeleeWeapon
  {
    public override void Action()
    {
      base.Action();

      if (!_isHit) return;
     
      if (_hit.collider.TryGetComponent(out BodyPart bodyPart))
      {
        var broadcaster = _hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
        broadcaster?.Hit(_unpin, Ray.direction * 200, _hit.point);

        bodyPart.TakeDamage();
      }
    }
  }
}