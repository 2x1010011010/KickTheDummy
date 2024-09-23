using Game.ResourceSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum LocationID
{
    MainMenu,
    Box,
    Stairs
}

public enum LocationState
{
    Unlocked,
    Locked,
    InDevelopment,
    Completed,
    Hidden
}

[CreateAssetMenu(fileName = "LocationEntity", menuName = "StaticData/LocationEntity", order = 1)]
public class LocationEntity : Resource
{
    [field: SerializeField, BoxGroup("DATA")] public LocationID LocationID { get; private set; }
    [field: SerializeField, BoxGroup("DATA")] public LocationState LocationState { get; private set; }

    [field: SerializeField, BoxGroup("RESOURCES")] public AssetReference SceneAssetReference { get; private set; }

    [field: SerializeField, BoxGroup("RESOURCES")] public string SceneName { get; private set; }
    [field: SerializeField, BoxGroup("RESOURCES")] public string Description { get; private set; }
    [field: SerializeField, BoxGroup("RESOURCES")] public Sprite Icon { get; private set; }

    public void SetState(LocationState locationState)
    {
        LocationState = locationState;
    }
}
