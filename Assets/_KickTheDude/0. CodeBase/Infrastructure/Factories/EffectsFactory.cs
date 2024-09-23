using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class EffectsFactory : IEffectsFactory
{
    private DiContainer _diContainer;
    private IAssetProvider _assetProvider;
    private IStaticDataService _staticDataService;

    private Dictionary<AssetReferenceGameObject, Pool> _pools = new Dictionary<AssetReferenceGameObject, Pool>();

    [Inject]
    public EffectsFactory(DiContainer diContainer, IAssetProvider assetProvider, IStaticDataService staticDataService)
    {
        _diContainer = diContainer;
        _assetProvider = assetProvider;
        _staticDataService = staticDataService;
    }

    private async UniTask<VisualEffect> CreateEntity(AssetReferenceGameObject reference, Transform parent = null)
    {
        var loadedObject = await _assetProvider.Load<GameObject>(reference);
        var createdObject = _diContainer.InstantiatePrefab(loadedObject, parent).GetComponent<VisualEffect>();

        return createdObject;
    }

    public async UniTask<VisualEffect> CreateEffect(EffectEntity effectEntity, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        VisualEffect createdObject = null;

        if (_pools.ContainsKey(effectEntity.EffectPrefabReference))
        {
            if (_pools.TryGetValue(effectEntity.EffectPrefabReference, out Pool objectPool))
            {
                if (objectPool.TryGetPoolable(out IPoolable poolable))
                {
                    createdObject = (VisualEffect)poolable;
                }
                else
                {
                    createdObject = await CreateEntity(effectEntity.EffectPrefabReference);
                    objectPool.Expand(createdObject);
                }
            }
        }
        else
        {
            var pool = new Pool(15);
            pool.Initialize();

            _pools.Add(effectEntity.EffectPrefabReference, pool);

            createdObject = await CreateEntity(effectEntity.EffectPrefabReference);

            pool.Expand(createdObject);
        }

        PlaceEffect(createdObject.transform, position, rotation, parent);

        return createdObject;
    }

    public async UniTask CreateEffectByPhysicsMaterial(PhysicMaterial physicsMaterial, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var surface = _staticDataService.GetSurfaceByMaterial(physicsMaterial);

        if (surface != null)
            await CreateEffect(surface.VisualEffect, position, rotation, parent); 
    }

    private void PlaceEffect(Transform effect, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        effect.position = position;
        effect.rotation = rotation;
        effect.parent = parent;
    }
}
