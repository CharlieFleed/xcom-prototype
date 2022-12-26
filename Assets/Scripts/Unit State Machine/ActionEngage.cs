using UnityEngine;
using System.Collections;
using HSM;
using DecisionTree;
using System.Collections.Generic;

public class ActionEngage : Action
{
    Unit _unit;
    DecisionTreeNode _decisionTree;

    FinalDecisionMoveToBestCover _moveToBestCoverDecision;
    FinalDecisionRunFromEnemies _runFromEnemiesDecision;
    DecisionCanIReachCover _canIReachCoverDecision;

    List<GridNode> _coverPositions;
    List<GridNode> _reachablePositions;

    ActionsController _actionsController;
    Walker _walker;
    GridEntity _gridEntity;

    public ActionEngage(Unit unit)
    {
        _unit = unit;
        _actionsController = unit.GetComponent<ActionsController>();
        _walker = unit.GetComponent<Walker>();
        _gridEntity = unit.GetComponent<GridEntity>();

        _coverPositions = new List<GridNode>();
        _reachablePositions = new List<GridNode>();

        // build a decision tree
        _moveToBestCoverDecision = new FinalDecisionMoveToBestCover(_unit, _coverPositions);
        _runFromEnemiesDecision = new FinalDecisionRunFromEnemies(_unit, _reachablePositions);
        _canIReachCoverDecision = new DecisionCanIReachCover(_unit, _coverPositions, _reachablePositions, _moveToBestCoverDecision, _runFromEnemiesDecision);

        _decisionTree = _canIReachCoverDecision;
    }

    public override void Execute()
    {
        // cancel current walk
        if (_walker.IsWalking)
        {
            Debug.Log($"{_unit.name} Stop walk.");
            _walker.Stop();
        }
        // add reaction
        if (_actionsController.NumActions == 0)
        {
            Debug.Log($"{_unit.name} add reaction.");
            _actionsController.NumActions = 1;
        }
        // add reaction event
        BattleEventEngageAction battleEventEngageAction = new BattleEventEngageAction(_actionsController, this, _gridEntity);
        NetworkMatchManager.Instance.AddBattleEvent(battleEventEngageAction, 0);
    }

    public void RunTree()
    { 
        ((FinalDecision)_decisionTree.MakeDecision()).Execute();
    }
}
