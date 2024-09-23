using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class ToolFactory : IEntitiesFactory<ITool>
{
    private DiContainer _diContainer;
    private IAssetProvider _assetProvider;
    private IStaticDataService _staticDataService;

    [Inject]
    public ToolFactory(DiContainer diContainer, IAssetProvider assetProvider, IStaticDataService staticDataService)
    {
        _diContainer = diContainer;
        _assetProvider = assetProvider;
        _staticDataService = staticDataService;
    }

    public async UniTask<ITool> CreateEntity(AssetReferenceGameObject reference, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var loadedObject = await _assetProvider.Load<GameObject>(reference);

        var createdObject = _diContainer.InstantiatePrefab(loadedObject, position, rotation, parent).GetComponent<ITool>();

        return createdObject;
    }

    public async UniTask<ITool> CreateEntity(AssetReferenceGameObject reference, Transform parent = null)
    {
        var loadedObject = await _assetProvider.Load<GameObject>(reference);

        var createdObject = _diContainer.InstantiatePrefab(loadedObject, parent).GetComponent<ITool>();

        return createdObject;
    }
}
