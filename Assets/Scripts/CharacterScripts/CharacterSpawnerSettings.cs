using UnityEngine;

namespace CharacterScripts
{
  [CreateAssetMenu(fileName = "SpawnerSettings", menuName = "Character/SpawnerSettings", order = 51)]
  public class CharacterSpawnerSettings : ScriptableObject
  {
    [field: SerializeField] public GameObject Prefab;
  }
}