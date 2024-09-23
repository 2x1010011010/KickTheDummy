using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UIFactory : IUIFactory
{
    private DiContainer _diContainer;
    private IAssetProvider _assetProvider;
    private IStaticDataService _staticDataService;

    [Inject]
    public UIFactory(DiContainer diContainer, IAssetProvider assetProvider, IStaticDataService staticDataService)
    {
        _diContainer = diContainer;
        _assetProvider = assetProvider;
        _staticDataService = staticDataService;
    }

    public async UniTask<T> CreateButton<T>(Transform parent) where T : UIButton
    {
        var loadedObject = await _assetProvider.Load<GameObject>(typeof(T).ToString());
        var instantiatedButton = _diContainer.InstantiatePrefab(loadedObject, parent).GetComponent<T>();

        return instantiatedButton;
    }

    public async UniTask<T> CreateWindow<T>(Transform parent) where T : UIWindow
    {
        var loadedObject = await _assetProvider.Load<GameObject>(typeof(T).ToString());
        var instantiatedWindow = _diContainer.InstantiatePrefab(loadedObject, parent).GetComponent<T>();

        _diContainer.Bind<T>().FromInstance(instantiatedWindow);

        return instantiatedWindow;
    }

    public void DestroyWindow(UIWindow obj)
    {
        _diContainer.Unbind(obj.GetType());
        _assetProvider.Cleanup();

        GameObject.Destroy(obj.gameObject);
    }
}
