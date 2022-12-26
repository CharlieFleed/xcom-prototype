using UnityEngine;
using System.Collections;

public static class ShotStatsHelper
{
    public static void UpdateShotStats(ShotStats shot, GridNode currentNode, Shooter shooter, bool overwatchShot)
    {
        shot.HitChance = 100 + shooter.Weapon.HitChanceBonus(shot.Target);
        if (overwatchShot)
        {
            shot.HitChance -= 15;
            Walker walker = shot.Target.GetComponent<Walker>();
            if (walker != null && walker.IsRunning)
                shot.HitChance -= 15;
        }
        else
        {
            if (shot.Cover)
                shot.HitChance -= 40;
            if (shot.HalfCover)
                shot.HitChance -= 20;
            if (shot.Flanked)
                shot.CritChance += 50;
            Hunkerer hunkerer = shot.Target.GetComponent<Hunkerer>();
            if (hunkerer != null && hunkerer.IsHunkering)
                shot.HitChance -= 30;
        }
        if (currentNode.Y > shot.Target.CurrentNode.Y)
            shot.HitChance += 20;

        shot.BaseDamagePreview = shooter.Weapon.BaseDamage;
        shot.MaxDamagePreview = shooter.Weapon.MaxDamage;

        shot.HitChance = Mathf.Clamp(shot.HitChance, 0, 100);
        shot.CritChance = Mathf.Clamp(shot.CritChance, 0, 100);
    }
    public static void UpdateMapEntityShotStats(ShotStats shot, Shooter shooter)
    {
        shot.HitChance = 100;
        shot.CritChance = 0;
        shot.BaseDamagePreview = shooter.Weapon.BaseDamage;
        shot.MaxDamagePreview = shooter.Weapon.MaxDamage;
    }
    public static void UpdateItemShotStats(ShotStats shot)
    {
        shot.HitChance = 100;
        shot.CritChance = 0;
        shot.BaseDamagePreview = 0;
        shot.MaxDamagePreview = 0;
        shot.Friendly = true;
    }

    public static bool OverwatchShot = true;
    public static bool RegularShot = false;
}
