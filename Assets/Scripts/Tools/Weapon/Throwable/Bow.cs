using CharacterScripts;
using RootMotion.Dynamics;
using UnityEngine;

namespace Tools.Weapon.Throwable
{
  public class Bow : ThrowableWeapon
  {
    public override void Action()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit = new RaycastHit();
      Physics.Raycast(ray, out hit, 100f);
      if (hit.collider.TryGetComponent(out BodyPart bodyPart))
      {
        var broadcaster = hit.collider.attachedRigidbody.GetComponent<MuscleCollisionBroadcaster>();
        broadcaster?.Hit(20f, ray.direction * 800f, hit.point);
        var spawned = Instantiate(Settings.Prefab, hit.point, Quaternion.LookRotation(-ray.direction));
        spawned.transform.SetParent(bodyPart.transform);
        bodyPart.TakeDamage();
      }
    }
  }
}