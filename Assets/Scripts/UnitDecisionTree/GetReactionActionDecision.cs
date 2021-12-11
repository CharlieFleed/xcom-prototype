using UnityEngine;
using System.Collections;
using DecisionTree;

public class GetReactionActionDecision : FinalDecision
{
    ActionsController _actionsController;

    public GetReactionActionDecision(ActionsController actionsController)
    {
        _actionsController = actionsController;
    }

    public override void Execute()
    {
        _actionsController.NumActions = 1;
    }
}
