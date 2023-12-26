using System;
using System.Collections.Generic;
using Infrastructure.StateMachine.States;
using Zenject;

namespace Infrastructure.StateMachine
{
  public class GameStateMachine
{
    private readonly Dictionary<Type, IState> _states;
    private IState _currentState;

    public GameStateMachine(DiContainer container)
    {
        _states = new Dictionary<Type, IState>()
        {
            [typeof(BootstrapState)] = InstantiateState<BootstrapState>(container),
            [typeof(LoadLevelState)] = InstantiateState<LoadLevelState>(container),
            [typeof(PreparationState)] = InstantiateState<PreparationState>(container),
            [typeof(GameState)] = InstantiateState<GameState>(container),
            [typeof(ReloadLevelState)] = InstantiateState<ReloadLevelState>(container),
            [typeof(GameExitState)] = InstantiateState<GameExitState>(container)
        };
    }

    private IState InstantiateState<TState>(DiContainer container) where TState : IState
    {
        return (IState)Activator.CreateInstance(typeof(TState), this, container);
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