using UnityEngine;
using System.Collections;
using HSM;

public class ConditionHaveIBeenShot : Condition
{
    Health _health;

    public ConditionHaveIBeenShot(Health health) : base()
    {
        _health = health;
    }

    public override bool Test()
    {
        return _health.TookDamage;
    }
}
