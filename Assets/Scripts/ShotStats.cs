using UnityEngine;

public class ShotStats
{
    public ShotStats()
    { }

    public GridEntity Target;
    public bool Available;
    public bool Flanked;
    public bool HalfCover;
    public bool Cover;
    public int HitChance;
    public int CritChance;
    public int BaseDamage;
    public int MaxDamage;
    public bool Friendly;
}