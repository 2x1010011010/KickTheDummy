using System.Collections.Generic;
using UnityEngine;

namespace Tools.Weapon.WeaponSettings
{
  [CreateAssetMenu(fileName = "GunSettings", menuName = "WeaponSettings/Gun", order = 51)]
  public class GunSettings : ScriptableObject
  {
    [field: SerializeField] public List<GameObject> BloodPrefab { get; private set; }
    [field: SerializeField] public float ReloadDelay { get; private set; }
    [field: SerializeField] public int ProjectileAmount { get; private set; }
    [field: SerializeField] public float Force { get; private set; }
  }
}