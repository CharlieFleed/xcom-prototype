using UnityEngine;
using System.Collections;
using DecisionTree;

public class FinalDecisionOverwatch : FinalDecision
{
    Overwatcher _overwatcher;

    public FinalDecisionOverwatch(Overwatcher overwatcher)
    {
        _overwatcher = overwatcher;
    }

    public override void Execute()
    {
        _overwatcher.Overwatch();
    }
}
