using System;
using Infrastructure.GameStateMachine.States;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class GameExitState : IState
  {
    public GameExitState(GameStateMachine gameStateMachine, DiContainer container)
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