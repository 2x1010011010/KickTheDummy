using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

namespace Game.ResourceSystem
{
    public enum EntityStatus
    {
        Unlocked,
        Locked,
        InDevelopment,
        Hidden
    }

    public enum EntityPossibilities
    {
        Explodeable,
        Piercer,
        Destructible,
        Dismemberment,
        Electro,
        Flying,
        Armored,
        Laser,
        Titan
    }

    public enum UnlockType
    {
        RewardADS,
        Buy
    }

    [CreateAssetMenu(fileName = "PropEntity", menuName = "StaticData/ResourceEntity", order = 1)]
    public class PropEntity : Resource
    {
        [field: SerializeField, BoxGroup("DATA")] public AssetReferenceGameObject InteractableObjectReference { get; private set; }
        [field: SerializeField, BoxGroup("DATA")] public List<EntityPossibilities> EntityPossibilities { get; private set; }
        [field: SerializeField, BoxGroup("VIEW")] public string ResourceName { get; private set; }
        [field: SerializeField, BoxGroup("VIEW")] public Sprite UIIcon { get; private set; }
    }
}
