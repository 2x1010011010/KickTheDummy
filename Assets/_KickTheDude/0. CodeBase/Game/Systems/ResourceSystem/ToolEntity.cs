using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum ToolCategory
{
    Interaction,
    Firearms,
    Props,
    Characters,
    Elements
}

[CreateAssetMenu(fileName = "ToolEntity", menuName = "StaticData/ToolEntity", order = 1)]
public class ToolEntity : Resource
{
    [field: SerializeField, BoxGroup("SETUP")] public AssetReferenceGameObject ToolReference { get; private set; }
    [field: SerializeField, BoxGroup("PARAMETERS")] public ToolCategory ToolCategory { get; private set; }
    [field: SerializeField, BoxGroup("UI")] public Sprite ToolIcon { get; private set; }
    [field: SerializeField, BoxGroup("UI")] public AudioClip SelectClip { get; private set; }
    [field: SerializeField, BoxGroup("UI")] public float BorderOffset { get; private set; }
    [field: SerializeField, BoxGroup("UI")] public float RotateAngle { get; private set; } = -45;
}
