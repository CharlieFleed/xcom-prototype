using UnityEngine;
using System.Collections;

namespace HSM
{
    public abstract class Condition
    {
        public abstract bool Test();
    }

    public delegate bool TestDelegate();

    public class DelegateCondition : Condition
    {
        TestDelegate _TestDel;

        public DelegateCondition(TestDelegate TestDel)
        {
            _TestDel = TestDel;
        }

        public override bool Test()
        {
            return _TestDel();
        }
    }

    public class AndCondition : Condition
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

    public class OrCondition : Condition
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

    public class NotCondition : Condition
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
