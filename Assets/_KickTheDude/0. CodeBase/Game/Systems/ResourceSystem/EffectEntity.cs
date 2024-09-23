using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "EffectEntity", menuName = "StaticData/EffectEntity", order = 1)]
public class EffectEntity : ScriptableObject
{
    [field: SerializeField] public AssetReferenceGameObject EffectPrefabReference { get; private set; }
    [field: SerializeField] public int PoolSize { get; private set; }
}
