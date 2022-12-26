using UnityEngine;
using System.Collections;
using DecisionTree;

public class FinalDecisionGetReactionAction : FinalDecision
{
    ActionsController _actionsController;

    public FinalDecisionGetReactionAction(ActionsController actionsController)
    {
        _actionsController = actionsController;
    }

    public override void Execute()
    {
        _actionsController.NumActions = 1;
    }
}
