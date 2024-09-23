using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class UIInstaller : MonoInstaller
{
    [SerializeField, BoxGroup("SETUP")] private Transform _windowsRoot;

    public override void InstallBindings()
    {
        var uiFactory = new UIFactory(Container.Resolve<DiContainer>(), Container.Resolve<IAssetProvider>(), Container.Resolve<IStaticDataService>());
        var audioSource = gameObject.AddComponent<AudioSource>();

        Container.Bind<IUIFactory>().FromInstance(uiFactory).AsSingle();
        Container.Bind<IUIService>().To<UIService>().FromInstance(new UIService(uiFactory, audioSource, _windowsRoot)).AsSingle().NonLazy();
    }
}
