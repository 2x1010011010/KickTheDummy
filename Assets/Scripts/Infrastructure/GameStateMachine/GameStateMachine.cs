using System;
using System.Collections.Generic;
using Infrastructure.GameStateMachine.States;
using UnityEngine;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class GameStateMachine
  {
    private readonly Dictionary<Type, IState> _states;
    private IState _currentState;

    public GameStateMachine(DiContainer container)
    {
      _states = new Dictionary<Type, IState>()
      {
        [typeof(BootstrapState)] = new BootstrapState(this, container),
        [typeof(LoadLevelState)] = new LoadLevelState(this, container),
        [typeof(PreparationState)] = new PreparationState(this, container),
        [typeof(GameState)] = new GameState(this, container),
        [typeof(ReloadLevelState)] = new ReloadLevelState(this, container),
        [typeof(GameExitState)] = new GameExitState(this, container),
      };
    }

    public void Enter<TState>() where TState : IState
    {
      _currentState?.Exit();
      var state = _states[typeof(TState)];
      _currentState = state;
      state.Enter(); 
    }
  }
}