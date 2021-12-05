using UnityEngine;
using System.Collections;
using DecisionTree;
using System.Collections.Generic;
using System.Linq;

public class RunFromEnemiesDecision : FinalDecision
{
    UnitLocalController _unit;
    List<GridNode> _reachablePositions;
    Walker _walker;
    GridAgent _gridAgent;
    GridEntity _gridEntity;
    TeamMember _teamMember;

    public RunFromEnemiesDecision(UnitLocalController unit, List<GridNode> reachablePositions)
    {
        _unit = unit;
        _reachablePositions = reachablePositions;
        //
        _walker = _unit.GetComponent<Walker>();
        _gridEntity = _unit.GetComponent<GridEntity>();
        _gridAgent = _unit.GetComponent<GridAgent>();
        _teamMember = _unit.GetComponent<TeamMember>();
    }

    public override void Execute()
    {
        Debug.Log("RunFromEnemies");
        GridNode destinationNode = _reachablePositions.OrderBy(n => ScorePosition(n)).First();
        Stack<GridNode> path = new Stack<GridNode>();
        path = Pathfinder.Instance.GetPathTo(destinationNode);
        _gridAgent.BookedNode = destinationNode;
        destinationNode.IsBooked = true;
        int cost = destinationNode.Distance <= _gridAgent.WalkRange ? 1 : 2;
        _walker.SetPath(path, cost);
    }

    float ScorePosition(GridNode gridNode)
    {
        float score = 0;
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(_teamMember);
        foreach (var enemy in enemies)
        {
            score += (enemy.CurrentNode.FloorPosition - gridNode.FloorPosition).magnitude;
        }
        return score;
    }
}
