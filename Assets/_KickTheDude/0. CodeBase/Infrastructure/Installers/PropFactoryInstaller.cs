using Game.InteractiveSystem;
using Zenject;

public class PropFactoryInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        BindFactory();
    }

    private void BindFactory()
    {
        Container.Bind<IEntitiesFactory<InteractableObject>>().To<PropFactory>().AsSingle();
    }
}
