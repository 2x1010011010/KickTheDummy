using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.GameStateMachine.States;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class GameState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
        //private readonly EventsFacade _eventsFacade;

        private readonly float _timeBetweenWaves;
        private CancellationTokenSource _cts;

        private readonly SavedData _savedData;

        private int _diedEnemiesCount;
        private int _maxMapEnemiesCount;
        private int _reward;
        private int _maxWaveCount;

        private readonly float _winRewardMultiplayer;
        
        public GameState(GameStateMachine gameStateMachine, DiContainer container)
        {
            _gameStateMachine = gameStateMachine;
            //_eventsFacade = container.Resolve<EventsFacade>();
            _savedData = container.Resolve<SavedData>();
            _timeBetweenWaves = container.Resolve<GameConfig>().DelayBetweenEnemyWaves;
            _winRewardMultiplayer = container.Resolve<GameConfig>().WinRewardMultiplayer;
        }

        public void Enter()
        {
            _diedEnemiesCount = 0;
            _reward = 0;
            SpawnWaves().Forget();
            SubscribeToEvents(true);
        }

        public void Exit()
        {
            _cts?.Cancel();
            SubscribeToEvents(false);
        }

        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
               // _eventsFacade.EnemyEvents.OnEnemyDeath += OnEnemyDeath;
               // _eventsFacade.EnemyEvents.OnTowerDie += LoseState;
            }
            else
            {
               // _eventsFacade.EnemyEvents.OnEnemyDeath -= OnEnemyDeath;
               // _eventsFacade.EnemyEvents.OnTowerDie -= LoseState;
            }
        }
        
        private void OnEnemyDeath()
        {
            _diedEnemiesCount++;
           // _reward += enemy.Data.Reward;
        }
        
        private async UniTaskVoid SpawnWaves()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            for (var i = 0; i <= _maxWaveCount; i++)
            {
              //  _eventsFacade.EnemyEvents.SpawnEnemiesWaveEvent(i, _maxWaveCount);
                await UniTask.Delay(TimeSpan.FromSeconds(_timeBetweenWaves), cancellationToken: _cts.Token);
            }
        }
  }
}