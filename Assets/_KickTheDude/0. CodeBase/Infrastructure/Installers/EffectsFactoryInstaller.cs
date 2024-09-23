using Zenject;

public class EffectsFactoryInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        BindEffectsFactory();
    }

    private void BindEffectsFactory()
    {
        Container.Bind<IEffectsFactory>().To<EffectsFactory>().AsSingle();
    }
}
