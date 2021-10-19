using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Overwatcher : BattleAction
{
    #region Fields

    [SerializeField] Shooter _shooter;

    bool _isOverwatching;
    // used by LookAtTarget
    public event Action<Shooter, GridEntity> OnShoot = delegate { };

    public override string ActionName { get { return _shooter.Weapon.Name + " " + base.ActionName; } }
    public override string ConfirmText { get { return _shooter.Weapon.Name + " " + base.ActionName; } }

    #endregion

    private void Awake()
    {
        Walker.OnMoveToNextNode += HandleWalker_MoveToNextNode;
    }

    void HandleWalker_MoveToNextNode(Walker walker, GridNode gridNode)
    {
        if (_isOverwatching && walker.GetComponent<Character>().Team != GetComponent<Character>().Team)
        {
            if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), walker.GetComponent<GridEntity>(), out Ray ray, out float rayLength, new List<GridNode[]>()))
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
        MatchManager.Instance.AddBattleEvent(shot, true);
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

    private void OnDestroy()
    {
        Walker.OnMoveToNextNode -= HandleWalker_MoveToNextNode;
    }
}
