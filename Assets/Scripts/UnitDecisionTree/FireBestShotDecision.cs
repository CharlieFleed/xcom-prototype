
using UnityEngine;
using System.Collections;
using DecisionTree;
using System.Collections.Generic;
using System.Linq;

public class FireBestShotDecision : FinalDecision
{
    Shooter _shooter;
    List<ShotStats> _shots;

    public FireBestShotDecision(Shooter shooter, List<ShotStats> shots)
    {
        _shooter = shooter;
        _shots = shots;
    }

    public override void Execute()
    {
        _shooter.ShootTarget(_shots.OrderBy(s => s.HitChance).First());
    }
}
