using UnityEngine;
using System.Collections;

namespace HSM
{
    public delegate bool TestDelegate();

    public abstract class ConditionBase
    {
        public abstract bool Test();
    }

    public class Condition : ConditionBase
    {
        TestDelegate _TestDel;

        public Condition(TestDelegate TestDel)
        {
            _TestDel = TestDel;
        }

        public override bool Test()
        {
            return _TestDel();
        }
    }

    public class AndCondition : ConditionBase
    {
        Condition _conditionA;
        Condition _conditionB;

        public AndCondition(Condition conditionA, Condition conditionB) : base()
        {
            _conditionA = conditionA;
            _conditionB = conditionB;
        }

        public override bool Test()
        {
            return _conditionA.Test() && _conditionB.Test();
        }
    }

    public class OrCondition : ConditionBase
    {
        Condition _conditionA;
        Condition _conditionB;

        public OrCondition(Condition conditionA, Condition conditionB) : base()
        {
            _conditionA = conditionA;
            _conditionB = conditionB;
        }

        public override bool Test()
        {
            return _conditionA.Test() || _conditionB.Test();
        }
    }

    public class NotCondition : ConditionBase
    {
        Condition _condition;

        public NotCondition(Condition condition) : base()
        {
            _condition = condition;
        }

        public override bool Test()
        {
            return !_condition.Test();
        }
    }
}
