﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;

public class Overwatcher : BattleAction
{
    #region Fields

    public static event Action<Overwatcher> OnOverwatcherAdded = delegate { };
    public static event Action<Overwatcher> OnOverwatcherRemoved = delegate { };

    [SerializeField] Shooter _shooter;

    bool _isOverwatching;
    public bool IsOverwatching { get { return _isOverwatching; } }
    // used by LookAtTarget
    public event Action<Shooter, GridEntity> OnShoot = delegate { };

    public override string ActionName { get { return _shooter.Weapon.Name + " " + base.ActionName; } }
    public override string ConfirmText { get { return _shooter.Weapon.Name + " " + base.ActionName; } }

    #endregion

    private void Start()
    {
        OnOverwatcherAdded(this);
    }

    private void OnDestroy()
    {
        OnOverwatcherRemoved(this);
    }

    private void OnEnable()
    {
        Walker.OnMove += HandleWalker_OnMove;
    }

    private void OnDisable()
    {
        Walker.OnMove -= HandleWalker_OnMove;
    }

    void HandleWalker_OnMove(Walker walker, GridNode origin, GridNode destination)
    {
        if (_isOverwatching && walker.GetComponent<Unit>().Team != GetComponent<Unit>().Team)
        {
            // if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), walker.GetComponent<GridEntity>(), out Ray ray, out float rayLength, new List<GridNode[]>()))
            if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), origin) && GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), destination))
            {
                if (Vector3.Distance(transform.position, walker.transform.position) <= _shooter.Weapon.Range)
                {
                    OverwatchShoot(walker.GetComponent<GridEntity>());
                }
            }
        }
    }

    // Update is called once per frame
    void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Overwatch();
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
    }

    public override void HandleConfirmClick()
    {
        Overwatch();
    }

    void Overwatch()
    {
        CmdOverwatch();
    }

    [Command]
    void CmdOverwatch()
    {
        RpcOverwatch();
    }

    [ClientRpc]
    void RpcOverwatch()
    {
        Debug.Log("Overwatcher RpcOverwatch");
        _isOverwatching = true;
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    void OverwatchShoot(GridEntity gridEntity)
    {
        //Debug.Log($"OverwatchShoot {name} in position {gridAgent.CurrentNode.X},{gridAgent.CurrentNode.Y},{gridAgent.CurrentNode.Z}");
        ShotStats shotStats = new ShotStats();
        shotStats.Target = gridEntity;
        shotStats.HitChance = 100 + _shooter.Weapon.HitChanceBonus(shotStats.Target);
        shotStats.HitChance = Mathf.Clamp(shotStats.HitChance, 0, 100);
        BattleEventShot shot = new BattleEventShot(_shooter, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shot, true);
        OnShoot(_shooter, gridEntity);
        ClearOverwatch();
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        ClearOverwatch();
        Available &= _shooter.Weapon.Bullets > 0;
    }

    void ClearOverwatch()
    {
        _isOverwatching = false;
    }
}
