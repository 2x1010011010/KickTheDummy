using Data.AdditionalData.Values;
using Infrastructure.Events;
using Infrastructure.Services.LoadingService;
using Infrastructure.Services.SaveService;
using Zenject;

namespace Infrastructure.StateMachine.States
{
  public class LoadLevelState : IState
  {
    
    private readonly StateMachine.GameStateMachine _gameStateMachine;
    private readonly DiContainer _container;
    //private readonly LoadingCurtain _loadingCurtain;
    private readonly EventsFacade _eventsFacade;
    private readonly SavedData _savedData;

    public LoadLevelState(StateMachine.GameStateMachine gameStateMachine, DiContainer container)
    {
      _gameStateMachine = gameStateMachine;
      _container = container;
      //_loadingCurtain = container.Resolve<LoadingCurtain>();
      _eventsFacade = container.Resolve<EventsFacade>();
      container.Resolve<SavedService>();
      _savedData = container.Resolve<SavedData>();
    }

    public void Enter()
    {
      LoadGameScene();
    }

    public void Exit()
    {
      _eventsFacade.GameEvents.BlockCameraEvent(true);
      _eventsFacade.GameEvents.LoadProgressDataEvent(_savedData);
      _eventsFacade.GameEvents.InitGameSceneEvent();
    }

    private void LoadGameScene()
    {
      var sceneLoader = _container.Resolve<SceneLoader>();
      var gameSceneName = Constance.Scenes.GameScene.ToString();

      //_loadingCurtain.Show();
      sceneLoader.LoadSceneAsync(gameSceneName, OnLoaded).Forget();
    }

    private void OnLoaded()
    {
      _gameStateMachine.Enter<GameState>();
      //_loadingCurtain.Hide();
    }
  }
}