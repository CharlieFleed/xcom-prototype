using UnityEngine;
using System.Collections;
using DecisionTree;

public class FinalDecisionReload : FinalDecision
{
    Reloader _reloader;
    public FinalDecisionReload(Reloader reloader)
    {
        _reloader = reloader;
    }

    public override void Execute()
    {
        _reloader.Reload();
    }
}
