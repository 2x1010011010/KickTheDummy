using Game.InteractiveSystem;
using NodeCanvas.Framework;

public class DestroyInteractableObject : ActionTask<InteractableObject>
{
    protected override string info
    {
        get { return string.Format("Destroy {0}", agentInfo); }
    }

    protected override void OnExecute()
    {
        agent.Destroy();
        EndAction();
    }
}
