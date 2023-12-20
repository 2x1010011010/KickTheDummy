
namespace Data.AdditionalData.Values
{
  public class Constance
  {
    
    public enum Scenes
    {
      InitialScene,
      GameScene
    }
    public enum Layers
    {
      Cell,
      Unit,
      UnitAttackArea,
      Enemy,
      EnemyAttackArea,
      Tower,
      Default
    }
    
    public const string SAVE_PROGRESS_KEY = "SaveData";
  }
  }
}