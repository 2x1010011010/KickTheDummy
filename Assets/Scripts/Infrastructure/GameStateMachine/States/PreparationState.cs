using Infrastructure.Events;
using Infrastructure.Services.SaveService;
using Zenject;

namespace Infrastructure.GameStateMachine.States
{
  public class PreparationState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly EventsFacade _eventsFacade;
    private readonly SavedService _savedService;

    public PreparationState(GameStateMachine gameStateMachine, DiContainer container)
    {
      _gameStateMachine = gameStateMachine;
      _eventsFacade = container.Resolve<EventsFacade>();
      _savedService = container.Resolve<SavedService>();
    }

    public void Enter()
    {
      _eventsFacade.HudEvents.OnPressPlayButton += StartFightState;
      _eventsFacade.GameEvents.BlockCameraEvent(false);
    }

    public void Exit()
    {
      _eventsFacade.HudEvents.OnPressPlayButton -= StartFightState;
    }

    private void StartFightState()
    {
      _gameStateMachine.Enter<GameState>();
      _savedService.SaveProgress();
    }
  }
}