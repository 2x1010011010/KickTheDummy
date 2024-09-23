using NodeCanvas.Framework;
using Game.InteractiveSystem;

public class ActivateInteractableObject : ActionTask<InteractableObject>
{
    protected override string info
    {
        get { return string.Format("Activate {0}", agentInfo); }
    }

    protected override void OnExecute()
    {
        //agent.ActivateInteraction();
        EndAction();
    }
}
