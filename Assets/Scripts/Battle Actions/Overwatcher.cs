using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;

public class Overwatcher : BattleAction
{
    #region Fields

    public static event Action<Overwatcher> OnOverwatcherAdded = delegate { };
    public static event Action<Overwatcher> OnOverwatcherRemoved = delegate { };
    public static event Action<Overwatcher> OnOverwatchShot = delegate { };

    [SerializeField] Shooter _shooter;
    GridEntity _gridEntity;

    bool _isOverwatching;
    public bool IsOverwatching { get { return _isOverwatching; } }
    // used by LookAtTarget
    public event Action<Shooter, GridEntity> OnShoot = delegate { };

    public override string ActionName { get { return _shooter.Weapon.Name + " " + base.ActionName; } }
    public override string ConfirmText { get { return base.ActionName; } }
    public override string DescriptionText { get { return _shooter.Weapon.Name + " " + base.ActionName; } }

    #endregion

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
    }

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
        if (!GetComponent<Health>().IsDead && _isOverwatching && walker.GetComponent<Unit>().Team != GetComponent<Unit>().Team)
        {
            // if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), walker.GetComponent<GridEntity>(), out Ray ray, out float rayLength, new List<GridNode[]>()))
            if (GridCoverManager.Instance.LineOfSight(_gridEntity, origin) && GridCoverManager.Instance.LineOfSight(_gridEntity, destination))
            {
                float distance = Vector3.Distance(_gridEntity.CurrentNode.FloorPosition, origin.FloorPosition);
                if (distance <= _shooter.Weapon.Range)
                {
                    OverwatchShoot(walker.GetComponent<GridEntity>());
                }
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (IsActive)
        {
            if (_input.GetKeyDown(KeyCode.Return))
            {
                Overwatch();
            }
            if (_input.GetKeyDown(KeyCode.Escape) || _input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
        _input.Clear();
    }

    public override void HandleConfirmClick()
    {
        Overwatch();
    }

    public void Overwatch()
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
        Debug.Log($"{name} Overwatcher RpcOverwatch");
        _isOverwatching = true;
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    void OverwatchShoot(GridEntity target)
    {
        Debug.Log($"OverwatchShoot {name} in position {_gridEntity.CurrentNode.X},{_gridEntity.CurrentNode.Y},{_gridEntity.CurrentNode.Z}");
        ShotStats shotStats = GridCoverManager.Instance.GetShotStats(_gridEntity, _gridEntity.CurrentNode, new List<GridEntity>() { target })[0];
        shotStats.HitChance = 100 + _shooter.Weapon.HitChanceBonus(shotStats.Target);
        Walker walker = target.GetComponent<Walker>();
        shotStats.HitChance -= 15;
        if (_gridEntity.CurrentNode.Y > target.CurrentNode.Y)
            shotStats.HitChance += 20;
        shotStats.HitChance = Mathf.Clamp(shotStats.HitChance, 0, 100);
        BattleEventShot shot = new BattleEventShot(_shooter, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shot, true, 2);
        OnShoot(_shooter, target);
        ClearOverwatch();
        OnOverwatchShot(this);
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
