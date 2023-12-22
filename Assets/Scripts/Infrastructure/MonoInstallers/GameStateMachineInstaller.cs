using Infrastructure.StateMachine.States;
using Infrastructure.StateMachine;
using Zenject;

namespace Infrastructure.MonoInstallers
{
  public class GameStateMachineInstaller : MonoInstaller, IInitializable
  {
    private StateMachine.GameStateMachine _gameStateMachine;
    public override void InstallBindings()
    {
      BindStateMachine();
      BindInterfaces();
    }

    public void Initialize()
    {
      _gameStateMachine.Enter<BootstrapState>();
    }

    private void BindStateMachine()
    {
      _gameStateMachine = new GameStateMachine(Container);
      Container.Bind<GameStateMachine>().FromInstance(_gameStateMachine).AsSingle().NonLazy();
    }

    private void BindInterfaces() =>
      Container.BindInterfacesTo<GameStateMachineInstaller>()
        .FromInstance(this);
  }
}