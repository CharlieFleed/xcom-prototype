using UnityEngine;
using System.Collections;
using HSM;
using DecisionTree;

public class SetDecisionTreeAction : ActionBase
{
    UnitDecisionTree _unitDecisionTree;
    DecisionTreeNode _decisionTreeNode;

    public SetDecisionTreeAction(UnitDecisionTree unitDecisionTree, DecisionTreeNode decisionTreeNode)
    {
        _unitDecisionTree = unitDecisionTree;
        _decisionTreeNode = decisionTreeNode;
    }

    public override void Execute()
    {
        _unitDecisionTree.SetDecisionTree(_decisionTreeNode);
    }
}
