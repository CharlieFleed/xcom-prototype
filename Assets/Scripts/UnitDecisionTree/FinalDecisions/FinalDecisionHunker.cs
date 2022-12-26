using UnityEngine;
using System.Collections;
using DecisionTree;

public class FinalDecisionHunker : FinalDecision
{
    Hunkerer _hunkerer;

    public FinalDecisionHunker(Hunkerer hunkerer)
    {
        _hunkerer = hunkerer;
    }

    public override void Execute()
    {
        _hunkerer.Hunker();
    }
}
