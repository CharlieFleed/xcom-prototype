using UnityEngine;
using System.Collections;
using HSM;

public class IsMySquadEngagedCondition : Condition
{
    SquadUnit _squadUnit;

    public IsMySquadEngagedCondition(SquadUnit squadUnit) : base()
    {
        _squadUnit = squadUnit;
    }

    public override bool Test()
    {
        return _squadUnit.Squad.Engaged;
    }
}
