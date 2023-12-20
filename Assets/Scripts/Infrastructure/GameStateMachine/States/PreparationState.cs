using System;
using Infrastructure.GameStateMachine.States;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class PreparationState : IState
  {
    public PreparationState(GameStateMachine gameStateMachine, DiContainer container)
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