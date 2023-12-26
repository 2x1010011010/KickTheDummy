using UnityEngine;

namespace Tools.Weapon.WeaponSettings
{
  [CreateAssetMenu(fileName = "ThrowableSettings", menuName = "WeaponSettings/Throwable", order = 0)]
  public class ThrowableSettings : ScriptableObject
  {
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public float Force { get; private set; }
  }
}