using UnityEngine;
using System.Collections;
using DecisionTree;
using System.Collections.Generic;
using System.Linq;

public class DecisionDoIHaveAGoodShot : Decision
{
    Shooter _shooter;
    List<ShotStats> _shots;

    public DecisionDoIHaveAGoodShot(Shooter shooter, List<ShotStats> shots, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
       base(trueNode, falseNode)
    {
        _shooter = shooter;
        _shots = shots;
    }

    public override DecisionTreeNode GetBranch()
    {
        Debug.Log("DoIHaveAGoodShot?");
        _shooter.UpdateShots();
        _shots.Clear();
        _shots.AddRange(_shooter.GetTargets().Where(s => { return (s.Available == true) && (s.Target.GetComponent<UnitLocalController>() != null) && (s.HitChance >= 50); }));
        Debug.Log($"Found {_shots.Count} shots.");
        if (_shots.Count > 0)
            return _trueNode;
        else
            return _falseNode;
    }
}
