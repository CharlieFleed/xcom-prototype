using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwatchFreezer : MonoBehaviour
{
    Movement _movement;
    Animator _animator;
    int _overwatchLocks;

    private void Awake()
    {
        _movement = gameObject.GetComponent<Movement>();
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        Overwatcher.OnOverwatchShot += Overwatcher_OnOverwatchShot;
        BattleEventShot.OnShootingEnd += BattleEventShot_OnShootingEnd;
    }

    private void OnDisable()
    {
        Overwatcher.OnOverwatchShot -= Overwatcher_OnOverwatchShot;
        BattleEventShot.OnShootingEnd -= BattleEventShot_OnShootingEnd;
    }

    private void Overwatcher_OnOverwatchShot(Overwatcher overwatcher)
    {
        if (overwatcher.gameObject != gameObject)
        {
            _overwatchLocks++;
            _movement.enabled = false;
            _animator.enabled = false;
        }
    }

    private void BattleEventShot_OnShootingEnd(Shooter arg1, GridEntity arg2)
    {
        if (_overwatchLocks > 0)
        {
            _overwatchLocks--;
        }
        if (_overwatchLocks == 0)
        {
            _movement.enabled = true;
            _animator.enabled = true;
        }
    }
}