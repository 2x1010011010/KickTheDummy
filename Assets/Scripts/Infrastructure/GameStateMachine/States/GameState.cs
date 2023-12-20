using System;
using Infrastructure.GameStateMachine.States;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class GameState : IState
  {
    public GameState(GameStateMachine gameStateMachine, DiContainer container)
    {
    }
    public void Enter()
    {
      throw new NotImplementedException();
    }

    public void Exit()
    {
      throw new NotImplementedException();
    }
  }
}