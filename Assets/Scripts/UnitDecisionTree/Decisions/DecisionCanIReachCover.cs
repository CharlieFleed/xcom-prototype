using UnityEngine;
using System.Collections;
using DecisionTree;
using System.Collections.Generic;

public class DecisionCanIReachCover : Decision
{
    Unit _unit;
    Walker _walker;
    GridEntity _gridEntity;
    GridAgent _gridAgent;
    ActionsController _actionsController;

    List<GridNode> _coverPositions;
    List<GridNode> _reachablePositions;

    public DecisionCanIReachCover(Unit unit, List<GridNode> coverPositions, List<GridNode> reachablePositions, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
        base(trueNode, falseNode)
    {
        _unit = unit;
        _coverPositions = coverPositions;
        _reachablePositions = reachablePositions;
        _walker = _unit.GetComponent<Walker>();
        _gridEntity = _unit.GetComponent<GridEntity>();
        _gridAgent = _unit.GetComponent<GridAgent>();
        _actionsController = _unit.GetComponent<ActionsController>();
    }

    bool IsNodeAvailable(GridNode node)
    {
        return GridManager.Instance.IsNodeAvailable(node, _gridAgent);
    }

    public override DecisionTreeNode GetBranch()
    {
        // get a list of all reachable nodes
        _walker.NumMoves = _actionsController.NumActions;
        GridNode _origin = _gridEntity.CurrentNode;
        int maxDistance = _gridAgent.WalkRange * _walker.NumMoves;
        float maxJumpUp = _gridAgent.MaxJumpUp;
        float maxJumpDown = _gridAgent.MaxJumpDown;
        Pathfinder.Instance.Initialize(GridManager.Instance.GetGrid(), _origin, _origin, maxDistance, maxJumpUp, maxJumpDown, IsNodeAvailable, true);
        _coverPositions.Clear();
        _reachablePositions.Clear();
        foreach (var node in GridManager.Instance.GetGrid().Nodes())
        {
            if (node.Distance < _gridAgent.WalkRange)
            {
                List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(_unit);
                int cover = GridCoverManager.Instance.GetCover(_gridEntity, node, enemies);
                if (cover > 0)
                    _coverPositions.Add(node);
                _reachablePositions.Add(node);
            }
        }
        if (_coverPositions.Count > 0)
            return _trueNode;
        else
            return _falseNode;
    }
}
