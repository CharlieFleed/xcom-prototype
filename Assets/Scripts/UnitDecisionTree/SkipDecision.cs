using UnityEngine;
using System.Collections;
using DecisionTree;

public class SkipDecision : FinalDecision
{
    Skipper _skipper;
    public SkipDecision(Skipper skipper)
    {
        _skipper = skipper;
    }

    public override void Execute()
    {
        _skipper.Skip();
    }
}