using DG.Tweening;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Gun : MonoBehaviour, IWeapon
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

    public void DestroyBlood(GameObject blood)
    {
      var s = DOTween.Sequence();
      s.AppendInterval(0.5f);
      s.AppendCallback(() => Destroy(blood));
    }
  }
}