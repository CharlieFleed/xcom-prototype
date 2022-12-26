using UnityEngine;
using System.Collections;
using HSM;

public class ConditionIsMySquadEngaged : Condition
{
    SquadUnit _squadUnit;

    public ConditionIsMySquadEngaged(SquadUnit squadUnit) : base()
    {
        _squadUnit = squadUnit;
    }

    public override bool Test()
    {
        return _squadUnit.Squad.Engaged;
    }
}
