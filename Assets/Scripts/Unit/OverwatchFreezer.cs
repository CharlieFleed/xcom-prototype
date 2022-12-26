using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwatchFreezer : MonoBehaviour
{
    [SerializeField] Animator _animator;

    Movement _movement;
    int _overwatchLocks;
    Unit _unit;

    private void Awake()
    {
        _movement = gameObject.GetComponent<Movement>();
        _unit = gameObject.GetComponent<Unit>();
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
        if (overwatcher.GetComponent<Unit>().Team != _unit.Team)
        {
            _overwatchLocks++;
            _movement.enabled = false;
            _animator.speed = 0;
            _animator.keepAnimatorControllerStateOnDisable = true;
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
            _animator.speed = 1;
        }
    }
}
