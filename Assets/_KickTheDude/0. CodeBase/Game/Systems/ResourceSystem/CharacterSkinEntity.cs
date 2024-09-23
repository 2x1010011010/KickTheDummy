using Game.ResourceSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public enum SkinID
{
    Dummy,
    Player,
    Juggernaut,
    Soldier,
    Driver,
    Fighter,
    ForestSoldier,
    SandJuggernaut,
    Meatbody,
    Zombie,
    GoldenBoy,
    RichDummy,
    TVman,
    CameraMan,
    SpeakerMan,
    Guardian,
    Jax,
    Pomni,
    Gangle,
    GamgleFemale,
    TitanCameraman,
    TitanSpeakerMan,
    TitanTVMan
}

[CreateAssetMenu(fileName = "ResourceEntity", menuName = "StaticData/CharacterSkin", order = 1)]
public class CharacterSkinEntity : PropEntity
{
    //[field: SerializeField, BoxGroup("SKIN DATA")] public SkinID SkinID { get; private set; }
}
