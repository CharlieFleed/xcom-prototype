using UnityEngine;
using System.Collections;
using DecisionTree;

public class DoIHaveAmmoDecision : Decision
{
    Weapon _weapon;

    public DoIHaveAmmoDecision(Weapon weapon, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
        base(trueNode, falseNode)
    {
        _weapon = weapon;
    }

    public override DecisionTreeNode GetBranch()
    {
        if (_weapon.InfiniteAmmo || _weapon.Bullets > 0)
            return _trueNode;
        else
            return _falseNode;
    }
}
