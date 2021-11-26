using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Explosive Data", menuName = "Explosive Data")]
public class ExplosiveData : ScriptableObject
{
    public string Name = "Explosive";
    public string Description = "Description";
    public Damage Damage;
    public int Radius = 3;
    public GameObject DetonationFXPrefab;
    public AudioClip DetonationAudioClip;
}
