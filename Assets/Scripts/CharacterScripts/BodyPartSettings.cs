using UnityEngine;

namespace CharacterScripts
{
  [CreateAssetMenu(fileName = "BodyPartSettings", menuName = "Character/BodyPartSettings", order = 0)]
  public class BodyPartSettings : ScriptableObject
  {
    [field: SerializeField] public float MinDamage { get; private set; }
    [field: SerializeField] public float MaxDamage { get; private set; }
  }
}