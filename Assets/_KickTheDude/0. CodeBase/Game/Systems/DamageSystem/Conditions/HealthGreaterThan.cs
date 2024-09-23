using NodeCanvas.Framework;
using Game.DamageSystem;

public class HealthGreaterThan : ConditionTask<HealthContainer>
{
    public float value;

    protected override bool OnCheck()
    {
        return agent.CurentHealth > value;
    }
}
