using Infrastructure.Events;
using Infrastructure.Services.SaveService;
using Zenject;

namespace Infrastructure.GameStateMachine.States
{
  public class ReloadLevelState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly EventsFacade _eventsFacade;
    private readonly SavedService _savedService;
    private readonly SavedData _savedData;

    public ReloadLevelState(GameStateMachine gameStateMachine, DiContainer container)
    {
      _gameStateMachine = gameStateMachine;
      _eventsFacade = container.Resolve<EventsFacade>();
      _savedService = container.Resolve<SavedService>();
      _savedData = container.Resolve<SavedData>();
    }

    public void Enter()
    {
      _eventsFacade.GameEvents.ClearLevel();
      _gameStateMachine.Enter<GameState>();
      _eventsFacade.GameEvents.BlockCameraEvent(true);
    }

    public void Exit()
    {
      _savedService.SaveProgress();
    }
  }
}