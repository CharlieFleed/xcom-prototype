using DecisionTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionAmIFlanked : Decision
{
    GridAgent _gridAgent;

    public DecisionAmIFlanked(GridAgent gridAgent, DecisionTreeNode trueNode, DecisionTreeNode falseNode) : 
        base(trueNode, falseNode)
    {
        _gridAgent = gridAgent;
    }

    public override DecisionTreeNode GetBranch()
    {
        if (_gridAgent.Cover == -1)
            return _trueNode;
        else
            return _falseNode;
    }
}
