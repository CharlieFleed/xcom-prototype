using UnityEngine;
using System.Collections;

namespace DecisionTree
{
    public abstract class DecisionTreeNode
    {
        public abstract DecisionTreeNode MakeDecision(); 
    }

    public abstract class FinalDecision : DecisionTreeNode
    {
        public override DecisionTreeNode MakeDecision()
        {
            return this;
        }

        public abstract void Execute();
    }

    public abstract class Decision : DecisionTreeNode
    {
        protected DecisionTreeNode _trueNode;
        protected DecisionTreeNode _falseNode;

        public Decision(DecisionTreeNode trueNode, DecisionTreeNode falseNode)
        {
            _trueNode = trueNode;
            _falseNode = falseNode;
        }

        abstract public DecisionTreeNode GetBranch();

        public override DecisionTreeNode MakeDecision()
        {
            DecisionTreeNode branch = GetBranch();
            return branch.MakeDecision();
        }
    }

    public delegate float FloatTestValueDelegate();

    public class FloatDecision : Decision
    {
        FloatTestValueDelegate _TestValue;
        float _minValue;
        float _maxValue;

        public FloatDecision(DecisionTreeNode trueNode, DecisionTreeNode falseNode, float minValue, float maxValue, FloatTestValueDelegate TestValue) :
            base(trueNode, falseNode)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _TestValue = TestValue;
        }

        float TestValue()
        {
            return _TestValue();
        }

        public override DecisionTreeNode GetBranch()
        {
            if (_maxValue >= TestValue() && TestValue() >= _minValue)
            {
                return _trueNode;
            }
            else
            {
                return _falseNode;
            }
        }
    }
}
