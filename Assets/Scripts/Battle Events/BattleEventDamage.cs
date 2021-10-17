using UnityEngine;
using System.Collections;

public class BattleEventDamage : BattleEvent
{
    float _waitTimeout = 2;

    public BattleEventDamage() : base()
    {
    }

    public override void Run()
    {
        base.Run();
        _waitTimeout -= Time.deltaTime;
        if (_waitTimeout <= 0)
        {
            End();
        }
    }
}
