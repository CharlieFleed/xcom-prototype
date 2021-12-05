using UnityEngine;
using System.Collections;
using DecisionTree;

public class ReloadDecision : FinalDecision
{
    Reloader _reloader;
    public ReloadDecision(Reloader reloader)
    {
        _reloader = reloader;
    }

    public override void Execute()
    {
        _reloader.Reload();
    }
}
