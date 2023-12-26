using Infrastructure.Events;
using Infrastructure.Services.SaveService;
using Zenject;

namespace Infrastructure.StateMachine.States
{
  public class GameExitState : IState
  {
    private readonly StateMachine.GameStateMachine _gameStateMachine;
    private readonly EventsFacade _eventsFacade;
    private readonly SavedData _savedData;
    private readonly SavedService _savedService;

    public GameExitState(StateMachine.GameStateMachine gameStateMachine, DiContainer container)
    {
      _gameStateMachine = gameStateMachine;

      _savedData = container.Resolve<SavedData>();
      _savedService = container.Resolve<SavedService>();
      _eventsFacade = container.Resolve<EventsFacade>();
      _eventsFacade.GameEvents.OnExitGame += OnExitGame;
    }
        
    private void OnExitGame()
    {
      _gameStateMachine.Enter<GameExitState>();
    }

    public void Enter()
    {
      _eventsFacade.GameEvents.ClearLevel();
      //_savedData.Timing.GameExitTime = DateTimeOffset.UtcNow;
      _savedService.SaveProgress();
    }

    public void Exit()
    {
      _eventsFacade.GameEvents.OnExitGame -= OnExitGame;
    }
  }
}