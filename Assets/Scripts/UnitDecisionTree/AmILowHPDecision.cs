using UnityEngine;
using System.Collections;
using DecisionTree;

public class AmILowHPDecision : Decision
{
    Health _health;

    public AmILowHPDecision(Health health, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
        base(trueNode, falseNode)
    {
        _health = health;
    }

    public override DecisionTreeNode GetBranch()
    {
        if (_health.IsLow)
            return _trueNode;
        else
            return _falseNode;
    }
}
