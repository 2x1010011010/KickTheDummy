using System;
using Cysharp.Threading.Tasks;
using Game.InteractiveSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class PropFactory : IEntitiesFactory<InteractableObject> 
{
    public event Action<string, InteractableObject> EntityCreated;

    private DiContainer _diContainer;
    private IAssetProvider _assetProvider;
    private IStaticDataService _staticDataService;

    [Inject]
    public PropFactory(DiContainer diContainer, IAssetProvider assetProvider, IStaticDataService staticDataService)
    {
        _diContainer = diContainer;
        _assetProvider = assetProvider;
        _staticDataService = staticDataService;
    }

    public async UniTask<InteractableObject> CreateEntity(AssetReferenceGameObject reference, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var loadedObject = await _assetProvider.Load<GameObject>(reference);

        var createdObject = _diContainer.InstantiatePrefab(loadedObject, position, rotation, parent).GetComponent<InteractableObject>();

        EntityCreated?.Invoke("", createdObject);

        return createdObject;
    }

    public async UniTask<InteractableObject> CreateEntity(AssetReferenceGameObject reference, Transform parent = null)
    {
        var loadedObject = await _assetProvider.Load<GameObject>(reference);

        var createdObject = _diContainer.InstantiatePrefab(loadedObject, parent).GetComponent<InteractableObject>();

        EntityCreated?.Invoke("", createdObject);

        return createdObject;
    }
}
