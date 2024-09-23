using Zenject;

public class ToolFactoryInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        BindFactory();
    }

    private void BindFactory()
    {
        Container.Bind<IEntitiesFactory<ITool>>().To<ToolFactory>().AsSingle();
    }
}
