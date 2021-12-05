using UnityEngine;
using System.Collections;
using DecisionTree;

public class HunkerDecision : FinalDecision
{
    Hunkerer _hunkerer;

    public HunkerDecision(Hunkerer hunkerer)
    {
        _hunkerer = hunkerer;
    }

    public override void Execute()
    {
        _hunkerer.Hunker();
    }
}
