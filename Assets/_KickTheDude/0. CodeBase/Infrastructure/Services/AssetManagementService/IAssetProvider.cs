using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public interface IAssetProvider : IService
{
    Task<GameObject> Instantiate(string path, Vector3 at);
    Task<GameObject> Instantiate(string path);
    Task<T> Load<T>(AssetReference prefabReference) where T : class;
    void Cleanup();
    Task<T> Load<T>(string address) where T : class;
    void Initialize();
}