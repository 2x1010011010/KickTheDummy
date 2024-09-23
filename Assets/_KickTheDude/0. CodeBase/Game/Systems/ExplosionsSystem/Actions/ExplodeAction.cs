using NodeCanvas.Framework;
using Game.ExplosionSystem;
using Game.InteractiveSystem;
using UnityEngine;

public class ExplodeAction : ActionTask<InteractableObject>
{
    protected override string info
    {
        get { return string.Format("Explode {0}", agentInfo); }
    }

    protected override void OnExecute()
    {
        if(agent.TryGetInteractive(out IExplodeable explodeable))
        {
            explodeable.Explode();
        }

        EndAction();
    }
}
