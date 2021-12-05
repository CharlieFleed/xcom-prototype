using UnityEngine;
using System.Collections;
using DecisionTree;

public class OverwatchDecision : FinalDecision
{
    Overwatcher _overwatcher;

    public OverwatchDecision(Overwatcher overwatcher)
    {
        _overwatcher = overwatcher;
    }

    public override void Execute()
    {
        _overwatcher.Overwatch();
    }
}
