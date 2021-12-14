using UnityEngine;
using System.Collections;
using System;

public class BattleEventOverwatchShot : BattleEventShot
{
    public static event Action<Shooter, GridEntity> OnOverwatchShooting = delegate { };

    public BattleEventOverwatchShot(Shooter shooter, ShotStats shotStats) : base(shooter, shotStats)
    { }

    public override void Run()
    {
        if (_phase == Phase.Camera)
        {
            OnOverwatchShooting(_shooter, _shotStats.Target);
            _phase = Phase.Wait;
        }
        else
        {
            base.Run();
        }
    }
}
