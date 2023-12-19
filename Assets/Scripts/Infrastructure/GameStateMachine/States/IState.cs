namespace Infrastructure.GameStateMachine.States
{
  public interface IState
  {
    public void Enter();
    public void Exit();
  }
}