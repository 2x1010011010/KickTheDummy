using Game.DamageSystem;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTaked : ConditionTask<DamageSystem>
{
    bool damageTaked;

    protected override void OnEnable()
    {
        damageTaked = false;

        agent.DamageTaked += DamageTakedRact;
    }

    protected override void OnDisable()
    {
        agent.DamageTaked -= DamageTakedRact;
    }

    protected override bool OnCheck()
    {
        return damageTaked;
    }

    private void DamageTakedRact(Damage damage)
    {
        damageTaked = true;
    }
}
