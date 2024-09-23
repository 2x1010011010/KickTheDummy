using NodeCanvas.Framework;
using Game.DamageSystem;

public class HealthLessThan : ConditionTask<HealthContainer>
{
    public float value;

    protected override bool OnCheck()
    {
        return agent.CurentHealth < value;
    }
}
