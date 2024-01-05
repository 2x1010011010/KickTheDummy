using DG.Tweening;
using Tools.Weapon.WeaponSettings;
using UnityEngine;

namespace Tools.Weapon.Gun
{
  public class Gun : MonoBehaviour, IWeapon
  {
    [SerializeField] protected GunSettings Settings;
    protected float ElapsedTime = 0;
    protected Ray Ray;
    protected LayerMask layers;
    protected float Unpin = 10f;
    protected RaycastHit Hit;
    protected bool IsHit = false;
    protected bool CanShoot = true;

    private void Update()
    {
      if (CanShoot) return;
      ElapsedTime += Time.deltaTime;
      if (ElapsedTime >= Settings.ReloadDelay)
        CanShoot = true;
    }

    public virtual void Action()
    {
      Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      Hit = new RaycastHit();
      IsHit = Physics.Raycast(Ray, out Hit, 100f);
    }

    public void DestroyBlood(GameObject blood)
    {
      var s = DOTween.Sequence();
      s.AppendInterval(0.5f);
      s.AppendCallback(() => Destroy(blood));
    }
  }
}