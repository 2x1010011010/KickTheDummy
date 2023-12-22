using System;
using Infrastructure.GameStateMachine.States;
using Infrastructure.Services.SaveService;

namespace Infrastructure.Events
{
  public class GameEvents
  {
    public void LoadProgressDataEvent(SavedData savedData) => OnLoadProgressData?.Invoke(savedData);

    public Action<SavedData> OnLoadProgressData;
    public void InitGameSceneEvent() => OnInitGameScene?.Invoke();

    public Action OnInitGameScene;
    public void ExitGameEvent() => OnExitGame?.Invoke();
    public Action OnExitGame;
    public void EnterGameState(IState state) => OnEnterGameState?.Invoke(state);
    public event Action<IState> OnEnterGameState;
    public void ExitGameState(IState state) => OnExitGameState?.Invoke(state);
    public event Action<IState> OnExitGameState;
    public void BlockCameraEvent(bool isBlock) => OnBlockCamera?.Invoke(isBlock);
    public event Action<bool> OnBlockCamera;

    public void MoveCameraToPreviewEvent() => OnMoveCameraToPreview?.Invoke();
    public event Action OnMoveCameraToPreview;
    
    public void ClearLevel() => OnClearLevel?.Invoke();
    public Action OnClearLevel;
  }
}