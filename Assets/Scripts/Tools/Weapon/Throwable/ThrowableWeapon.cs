using Tools.Weapon.WeaponSettings;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools.Weapon.Throwable
{
  public class ThrowableWeapon : MonoBehaviour, IWeapon
  {
    [SerializeField] protected ThrowableSettings Settings;
    [SerializeField] private Vector3 _spawnOffset;

    private float _elapsedTime = 0.0f;
    private float _nextdropDelay = 2.0f;
    protected bool IsDroped = false;
    
    private void Update()
    {
      _elapsedTime += Time.deltaTime;
      if (_elapsedTime > _nextdropDelay)
        IsDroped = false;
    }

    public virtual void Action()
    {
      if (IsDroped) return;

      IsDroped = true;
      _elapsedTime = 0.0f;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      var spawned = Instantiate(Settings.Prefab, transform.position + transform.rotation * _spawnOffset, transform.rotation);
      var rb = spawned.GetComponent<Rigidbody>();
      rb.mass = Settings.Mass;
      rb.AddForce(ray.direction * Settings.Force, ForceMode.Impulse);
    }
  }
}