using UnityEngine;
using System.Collections;
using DecisionTree;

public class DecisionDoIHaveAmmo : Decision
{
    Weapon _weapon;

    public DecisionDoIHaveAmmo(Weapon weapon, DecisionTreeNode trueNode, DecisionTreeNode falseNode) :
        base(trueNode, falseNode)
    {
        _weapon = weapon;
    }

    public override DecisionTreeNode GetBranch()
    {
        if (_weapon != null && (_weapon.InfiniteAmmo || _weapon.Bullets > 0))
            return _trueNode;
        else
            return _falseNode;
    }
}
