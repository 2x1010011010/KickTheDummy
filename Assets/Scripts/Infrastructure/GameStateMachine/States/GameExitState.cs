using System;
using Infrastructure.GameStateMachine.States;
using Infrastructure.Services;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class GameExitState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly EventsFacade _eventsFacade;
    private readonly SavedData _savedData;
    private readonly SavedService _savedService;

    public GameExitState(GameStateMachine gameStateMachine, DiContainer container)
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
      _savedData.Timing.GameExitTime = DateTimeOffset.UtcNow;
      _savedService.SaveProgress();
    }

    public void Exit()
    {
      _eventsFacade.GameEvents.OnExitGame -= OnExitGame;
    }
  }
}