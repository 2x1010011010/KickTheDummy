using System;
using Infrastructure.GameStateMachine.States;
using Zenject;

namespace Infrastructure.GameStateMachine
{
  public class ReloadLevelState : IState
  {
    public ReloadLevelState(GameStateMachine gameStateMachine, DiContainer container)
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