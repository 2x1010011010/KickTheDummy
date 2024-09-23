using NodeCanvas.Framework;
using Game.DamageSystem;

public class HealthEnded : ConditionTask<HealthContainer>
{
    protected override bool OnCheck()
    {
        return agent.IsHealthEnded;
    }
}
