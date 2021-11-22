using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Grenade Data", menuName = "Grenade Data")]
public class GrenadeData : ScriptableObject
{
    public string Name = "Grenade";
    public string Description = "Description";
    public Damage Damage;
    public int Radius = 10;
    public GameObject DetonationFXPrefab;
    public AudioClip DetonationAudioClip;
    public Sprite Image;
}
