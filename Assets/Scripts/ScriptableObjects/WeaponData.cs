using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string Name = "Weapon";
    public Damage Damage;
    public int Range = 10;
    public int ClipSize = 4;
    public GameObject ShootFXPrefab;
    public GameObject HitFXPrefab;
    public AudioClip ShootAudioClip;
    public bool InfiniteAmmo = false;
    public Sprite Image;
    public WeaponType WeaponType;
}
