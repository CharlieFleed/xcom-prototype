using UnityEngine;
using System.Collections;
using DecisionTree;
using System.Collections.Generic;
using System.Linq;

public class CanIReachBetterCoverDecision : Decision
{
    Unit _unit;
    GridEntity _gridEntity;
    GridAgent _gridAgent;
    Shooter _shooter;

    List<GridNode> _coverPositions;

    public CanIReachBetterCoverDecision(Unit unit, List<GridNode> coverPositions, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
        base(trueNode, falseNode)
    {
        _unit = unit;
        _coverPositions = coverPositions;
        _gridEntity = _unit.GetComponent<GridEntity>();
        _gridAgent = _unit.GetComponent<GridAgent>();
        _shooter = _unit.GetComponent<Shooter>();
    }

    public override DecisionTreeNode GetBranch()
    {
        GridNode bestCover = _coverPositions.OrderByDescending(n => ScorePosition(n)).First();
        if (ScorePosition(bestCover) > ScorePosition(_gridEntity.CurrentNode))
            return _trueNode;
        else
            return _falseNode;
    }

    float ScorePosition(GridNode gridNode)
    {
        float score = 1;
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(_unit);
        int cover = GridCoverManager.Instance.GetCover(_gridEntity, gridNode, enemies);
        switch (cover)
        {
            case -1:
                score *= .2f;
                break;
            case 0:
                break;
            case 1:
                score *= 1.2f;
                break;
            case 2:
                score *= 1.5f;
                break;
            default:
                break;
        }
        int cost = gridNode.Distance <= _gridAgent.WalkRange ? 1 : 2;
        {
            if (cost == 1)
                score *= 1.5f;
        }
        Queue<ShotStats> shots = _shooter.GetShotsFromPosition(gridNode);
        foreach (var shot in shots)
        {
            if (shot.Available && shot.Flanked)
            {
                score *= 2;
                break;
            }
        }
        return score;
    }
}
