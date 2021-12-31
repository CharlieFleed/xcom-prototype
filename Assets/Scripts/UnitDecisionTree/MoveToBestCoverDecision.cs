using UnityEngine;
using System.Collections;
using DecisionTree;
using System.Collections.Generic;
using System.Linq;

public class MoveToBestCoverDecision : FinalDecision
{
    Unit _unit;
    Walker _walker;
    GridAgent _gridAgent;
    GridEntity _gridEntity;
    Shooter _shooter;
    List<GridNode> _nodes;

    public MoveToBestCoverDecision(Unit unit, List<GridNode> nodes)
    {
        _unit = unit;
        _nodes = nodes;
        //
        _walker = _unit.GetComponent<Walker>();
        _gridEntity = _unit.GetComponent<GridEntity>();
        _gridAgent = _unit.GetComponent<GridAgent>();
        _shooter = _unit.GetComponent<Shooter>();
    }

    public override void Execute()
    {
        Debug.Log("MoveToBestCover");
        GridNode destinationNode = _nodes.OrderByDescending(n => ScorePosition(n)).First();
        foreach (var node in _nodes.OrderByDescending(n => ScorePosition(n)))
        {
            //Debug.Log($"node: {node.X},{node.Y},{node.Z} score: {ScorePosition(node)}.");
        }
        Stack<GridNode> path = new Stack<GridNode>();
        path = Pathfinder.Instance.GetPathTo(destinationNode);
        _gridAgent.BookedNode = destinationNode;
        destinationNode.IsBooked = true;
        int cost = destinationNode.Distance <= _gridAgent.WalkRange ? 1 : 2;
        Debug.Log($"Move to: {destinationNode.X},{destinationNode.Y},{destinationNode.Z}.");
        _walker.SetPath(path, cost);
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
