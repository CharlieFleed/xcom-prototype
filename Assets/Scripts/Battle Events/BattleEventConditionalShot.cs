using UnityEngine;
using System.Collections;

public class BattleEventConditionalShot : BattleEventShot
{
    public BattleEventConditionalShot(Shooter shooter, ShotStats shotStats) : base(shooter, shotStats)
    { }

    public override void Run()
    {
        if (_shooter.GetComponent<Health>().IsDead || _shotStats.Target.GetComponent<Health>().IsDead)
        {
            End();
        }
        base.Run();
    }
}
