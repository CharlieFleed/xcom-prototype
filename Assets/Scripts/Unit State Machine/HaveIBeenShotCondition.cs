using UnityEngine;
using System.Collections;
using HSM;

public class HaveIBeenShotCondition : Condition
{
    Health _health;

    public HaveIBeenShotCondition(Health health) : base()
    {
        _health = health;
    }

    public override bool Test()
    {
        return _health.TookDamage;
    }
}
