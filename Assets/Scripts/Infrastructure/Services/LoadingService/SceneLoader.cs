using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.LoadingService
{
  public class SceneLoader
  {
    private CancellationTokenSource _cts;
    public async UniTaskVoid LoadSceneAsync(string nextScene, Action onLoaded = null)
    {
      _cts?.Cancel();
      _cts = new CancellationTokenSource();
      AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);

      while (!waitNextScene.isDone)
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: _cts.Token);
            
      onLoaded?.Invoke();
    }
  }
}