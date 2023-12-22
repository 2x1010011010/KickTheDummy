using Data.AdditionalData.Values;
using Infrastructure.Services.LoadingService;
using Infrastructure.Services.SaveService;
using UnityEngine;
using Zenject;

namespace Infrastructure.StateMachine.States
{
  public class BootstrapState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly SavedService _savedService;

    public BootstrapState(StateMachine.GameStateMachine gameStateMachine, DiContainer container)
    {
    }

    public void Enter()
    {
      Application.targetFrameRate = 60;
      LoadInitialScene();
      _savedService.LoadProgress();
    }

    public void Exit()
    {

    }
    
    private void LoadInitialScene()
    {
      var initialSceneName = Constance.Scenes.InitialScene.ToString();
      _sceneLoader.LoadSceneAsync(initialSceneName, onLoaded: EnterLoadLevel).Forget();
    }

    private void EnterLoadLevel()
    {
      _gameStateMachine.Enter<LoadLevelState>();
    }
  }
}