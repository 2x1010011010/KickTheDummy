using CharacterScripts;
using UnityEngine;

namespace Tools.Weapon.Melee
{
  public class CharacterTool : MeleeWeapon
  {
    [SerializeField] private CharacterSpawner _spawner;
    
    public override void Action()
    {
      _spawner.Spawn();
    }
  }
}