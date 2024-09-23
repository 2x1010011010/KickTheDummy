using NodeCanvas.Framework;
using Game.InteractiveSystem;

public class DeactivateInteractableObject : ActionTask<InteractableObject>
{
    protected override string info
    {
        get { return string.Format("Deactivate {0}", agentInfo); }
    }

    protected override void OnExecute()
    {
        //agent.DeactivateInteraction();
        EndAction();
    }
}
