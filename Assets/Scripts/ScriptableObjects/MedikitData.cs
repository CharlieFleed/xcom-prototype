using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Medikit Data", menuName = "Medikit Data")]
public class MedikitData : ScriptableObject
{
    public string Name = "Medikit";
    public float Range = 1;
    public Damage Damage;
    public GameObject EffectFXPrefab;
}
