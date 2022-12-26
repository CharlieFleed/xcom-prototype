using UnityEngine;
using System.Collections;
using DecisionTree;

public class FinalDecisionSkip : FinalDecision
{
    Skipper _skipper;
    public FinalDecisionSkip(Skipper skipper)
    {
        _skipper = skipper;
    }

    public override void Execute()
    {
        _skipper.Skip();
    }
}