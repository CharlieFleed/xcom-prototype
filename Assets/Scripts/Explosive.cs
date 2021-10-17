using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] int _damage = 4;
    [SerializeField] int _radius = 10;
    [SerializeField] GameObject _detonationFXPrefab;

    public int Damage { get { return _damage; } set { _damage = value; } }
    public int Radius { get { return _radius; } set { _radius = value; } }
    public GameObject DetonationFXPrefab { get { return _detonationFXPrefab; } set { _detonationFXPrefab = value; } }

    private void Awake()
    {
        GetComponent<Health>().OnDied += HandleExplosive_OnDied;
    }

    private void HandleExplosive_OnDied()
    {
        BattleEventExplosion _explosion = new BattleEventExplosion(this);
        MatchManager.Instance.AddBattleEvent(_explosion, true);
    }

    public void Detonate()
    {
        // show detonation FX
        Instantiate(_detonationFXPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
