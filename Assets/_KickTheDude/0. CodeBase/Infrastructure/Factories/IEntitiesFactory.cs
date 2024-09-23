using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IEntitiesFactory<T> : IFactory
{
    UniTask<T> CreateEntity(AssetReferenceGameObject reference, Transform parent = null);
    UniTask<T> CreateEntity(AssetReferenceGameObject reference, Vector3 position, Quaternion rotation, Transform parent = null);
}
