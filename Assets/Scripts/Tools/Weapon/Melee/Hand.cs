using CharacterScripts;
using RootMotion.Dynamics;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.Weapon.Melee
{
  public class Hand : MeleeWeapon
  {
    private Rigidbody _draggedPart;
    private PuppetMaster _puppetMaster;
    private Vector3 _mouseDirection;
    public bool Dragged;

    private void Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        Dragged = false;
        _draggedPart = null;
        _isHit = false;
      }
    }

    public override void Action()
    {
      base.Action();
      
      if (Dragged)
      {
        Debug.Log("Dragged");
        _mouseDirection = Input.GetTouch(0).deltaPosition.normalized;
        _draggedPart.AddForce(_mouseDirection * 5, ForceMode.Impulse);
        var broadcaster = _draggedPart.GetComponent<MuscleCollisionBroadcaster>();
        broadcaster?.Hit(5, _mouseDirection * 3, _draggedPart.position);
      }

      if (!Input.GetMouseButton(0)) return;
      if (!_isHit) return;
      if (!_hit.collider.TryGetComponent(out BodyPart bodyPart)) return;
      GetRigidBodyComponent(bodyPart);
      
    }

    private void GetRigidBodyComponent(BodyPart bodyPart)
    {
      if (Dragged) return; 
      Dragged = true;
      _draggedPart = bodyPart.GetComponent<Rigidbody>();
      _puppetMaster = _draggedPart.GetComponentInParent<PuppetMaster>();
    }
  }
}