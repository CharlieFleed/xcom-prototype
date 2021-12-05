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

    [SerializeField] Shooter _shooter;

    bool _isOverwatching;
    public bool IsOverwatching { get { return _isOverwatching; } }
    // used by LookAtTarget
    public event Action<Shooter, GridEntity> OnShoot = delegate { };

    public override string ActionName { get { return _shooter.Weapon.Name + " " + base.ActionName; } }
    public override string ConfirmText { get { return base.ActionName; } }
    public override string DescriptionText { get { return _shooter.Weapon.Name + " " + base.ActionName; } }

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
        if (!GetComponent<Health>().IsDead && _isOverwatching && walker.GetComponent<TeamMember>().Team != GetComponent<TeamMember>().Team)
        {
            // if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), walker.GetComponent<GridEntity>(), out Ray ray, out float rayLength, new List<GridNode[]>()))
            if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), origin) || GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), destination))
            {
                if (Vector3.Distance(transform.position, walker.transform.position) <= _shooter.Weapon.Range)
                {
                    OverwatchShoot(walker.GetComponent<GridEntity>());
                }
            }
        }
    }

    private void Update()
    {
        if (IsActive)
        {
            _input.Update();
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
