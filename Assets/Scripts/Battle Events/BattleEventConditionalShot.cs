using UnityEngine;
using System.Collections;

public class BattleEventConditionalShot : BattleEventShot
{
    public BattleEventConditionalShot(Shooter shooter, ShotStats shotStats) : base(shooter, shotStats)
    { }

    public override void Run()
    {
        //Debug.Log($"Run BattleEventConditionalShot");
        if (_shooter.GetComponent<Health>().IsDead || _shotStats.Target.GetComponent<Health>().IsDead)
        {
            //Debug.Log("Canceling shot.");
            End();
        }
        else
        {
            //Debug.Log("Confirm shot.");
            base.Run();
        }
    }
}
