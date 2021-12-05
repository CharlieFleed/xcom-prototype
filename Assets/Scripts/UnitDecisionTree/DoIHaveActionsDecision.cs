using UnityEngine;
using System.Collections;
using DecisionTree;

public class DoIHaveActionsDecision : Decision
{
    ActionsController _actionsController;

    public DoIHaveActionsDecision(ActionsController actionsController, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
        base(trueNode, falseNode)
    {
        _actionsController = actionsController;
    }

    public override DecisionTreeNode GetBranch()
    {
        if (_actionsController.NumActions > 0)
            return _trueNode;
        else
            return _falseNode;
    }
}
