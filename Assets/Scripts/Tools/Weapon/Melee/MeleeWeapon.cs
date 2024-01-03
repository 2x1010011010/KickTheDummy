using UnityEngine;

namespace Tools.Weapon.Melee
{
  public class MeleeWeapon : MonoBehaviour, IWeapon
  {
    protected Ray Ray;
    protected LayerMask layers;
    protected float _unpin = 10f;
    protected RaycastHit _hit;
    protected bool _isHit = false;
    public virtual void Action()
    {
      Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      _hit = new RaycastHit();
      _isHit = Physics.Raycast(Ray, out _hit, 100f);
    }
  }
}