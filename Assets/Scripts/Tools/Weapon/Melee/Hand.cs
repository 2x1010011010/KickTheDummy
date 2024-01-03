using CharacterScripts;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Weapon.Melee
{
  public class Hand : MeleeWeapon
  {
    public bool Dragged;
    private void Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        Dragged = false;
      }

      if (!Input.GetMouseButton(0)) return;
      if (!_isHit) return;
      if (_hit.collider.TryGetComponent(out BodyPart bodyPart))
      {
        Dragged = true;
        bodyPart.transform.position = Input.mousePosition;
      }
    }

    public override void Action()
    {
    }
  }
}