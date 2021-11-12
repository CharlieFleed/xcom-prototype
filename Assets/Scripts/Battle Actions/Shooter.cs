using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;
using Mirror;


public class Shooter : BattleAction
{
    #region Fields

    public static event Action<Shooter> OnShooterAdded = delegate { };
    public static event Action<Shooter> OnShooterRemoved = delegate { };

    public event Action<ShotStats> OnTargetAdded = delegate { };
    public event Action OnHideTargets = delegate { };

    public event Action<Shooter, GridEntity> OnTargetSelected = delegate { };
    public event Action OnTargetingEnd = delegate { };

    public event Action OnShot = delegate { };
    public event Action<Shooter> OnShoot = delegate { };

    protected Queue<ShotStats> _targets = new Queue<ShotStats>();

    [SerializeField] protected Weapon _weapon;
    public Weapon Weapon { get { return _weapon; } set { _weapon = value; } }

    public override string ActionName { get { return _weapon.Name; } }
    public override string ConfirmText { get { return "Fire " + _weapon.Name; } }

    public static event Action<ShotStats> OnShotSelected = delegate { };

    #endregion

    private void OnEnable()
    {
        OnShooterAdded(this);
    }

    private void OnDisable()
    {
        OnShooterRemoved(this);
    }

    // Update is called once per frame
    void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                NextTarget();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Shoot();
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
    }

    protected virtual void Shoot()
    {
        if (_targets.Count > 0 && _targets.Peek().Available)
        {
            HideTargets();
            OnTargetingEnd();
            CmdShoot(_targets.Peek().Target.gameObject);
        }
    }

    [Command]
    protected void CmdShoot(GameObject target)
    {
        RpcShoot(target);
    }

    [ClientRpc]
    protected virtual void RpcShoot(GameObject target)
    {
        Debug.Log("Shooter RpcShoot");
        GetTargets();
        ShotStats shotStats = null;
        foreach (var shot in _targets)
        {
            if (shot.Target == target.GetComponent<GridEntity>())
            {
                shotStats = shot;
            }
        }
        if (shotStats == null) Debug.Log($"PANIC! target {target.name} not found!");
        OnTargetSelected(this, shotStats.Target);
        OnTargetingEnd();
        Debug.Log($"Shoot {shotStats.Target.name}");
        BattleEventShot shotEvent = new BattleEventShot(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shotEvent, true);
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    public void Shot()
    {
        OnShot();
    }

    public void DoShoot()
    {
        _weapon.Shoot();
        OnShoot(this);
    }

    void NextTarget()
    {
        if (_targets.Count > 0)
        {
            // unselect current target
            _targets.Peek().Target.IsTargeted = false;
            // rotate targets
            ShotStats target = _targets.Dequeue();
            _targets.Enqueue(target);
            // select new target
            _targets.Peek().Target.IsTargeted = true;
            OnTargetSelected(this, _targets.Peek().Target);
            OnShotSelected(_targets.Peek());
        }
    }

    override public void Activate()
    {
        //Debug.Log("Shooter - Activate");
        base.Activate();
        ShowTargets();
        StartTargetSelection();
    }

    override public void Cancel()
    {
        //Debug.Log("Shooter - Cancel");
        if (_targets.Count > 0)
        {
            _targets.Peek().Target.IsTargeted = false;
        }
        HideTargets();
        OnTargetingEnd();
        base.Cancel();        
    }

    void StartTargetSelection()
    {
        if (_targets.Count > 0)
        {
            _targets.Peek().Target.IsTargeted = true;
            OnTargetSelected(this, _targets.Peek().Target);
            OnShotSelected(_targets.Peek());
        }
    }

    public void GetTargets()
    {
        _targets.Clear();
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(GetComponent<Unit>());
        foreach (var shotStats in GridCoverManager.Instance.GetShotStats(this, enemies))
        {
            shotStats.Available = Vector3.Distance(transform.position, shotStats.Target.transform.position) <= _weapon.Range;
            shotStats.HitChance = 100 + Weapon.HitChanceBonus(shotStats.Target);
            if (shotStats.Cover)
                shotStats.HitChance -= 40;
            if (shotStats.HalfCover)
                shotStats.HitChance -= 20;
            if (shotStats.Flanked)
                shotStats.CritChance += 50;
            if (shotStats.Target.GetComponent<GridAgent>().Hunkering)
                shotStats.HitChance -= 30;
            shotStats.HitChance = Mathf.Clamp(shotStats.HitChance, 0, 100);
            shotStats.CritChance = Mathf.Clamp(shotStats.CritChance, 0, 100);
            shotStats.BaseDamage = Weapon.BaseDamage;
            shotStats.MaxDamage = Weapon.MaxDamage; 
            _targets.Enqueue(shotStats);
        }
        List<GridEntity> gridEntities = NetworkMatchManager.Instance.GetGridEntities();
        // Add map entities
        foreach (var gridEntity in gridEntities)
        {
            GridAgent gridAgent = gridEntity.GetComponent<GridAgent>();
            if (gridAgent == null)
            {
                if (GridCoverManager.Instance.LineOfSight(GetComponent<GridEntity>(), gridEntity, out Ray ray, out float rayLength, new List<GridNode[]>()))
                {
                    ShotStats shotStats = new ShotStats();
                    shotStats.Target = gridEntity;
                    shotStats.Available = Vector3.Distance(transform.position, gridEntity.transform.position) <= _weapon.Range;
                    shotStats.HitChance = 100;
                    shotStats.CritChance = 0;
                    shotStats.BaseDamage = Weapon.BaseDamage;
                    shotStats.MaxDamage = Weapon.MaxDamage;
                    _targets.Enqueue(shotStats);
                }
            }
        }
    }

    public void ShowTargets()
    {
        //Debug.Log($"Shooter - ShowTargets. #targets: {_targets.Count}.");
        foreach (var target in _targets)
        {
            OnTargetAdded(target);
        }
    }

    public void HideTargets()
    {
        foreach (var target in _targets)
        {
            target.Target.IsTargeted = false;
        }
        OnHideTargets();
    }

    public bool HasAvailableTargets()
    {
        foreach (var target in _targets)
        {
            if (target.Available)
                return true;
        }
        return false;
    }

    public void SelectTarget(ShotStats target)
    {
        if (_targets.Count > 0)
        {
            while (_targets.Peek().Target != target.Target)
            {
                NextTarget();
            }
        }
    }

    public void ShootRandomTarget()
    {
        List<ShotStats> availableShots = new List<ShotStats>();
        foreach (var shot in _targets)
        {
            if (shot.Available)
                availableShots.Add(shot);
        }
        ShotStats selectedShot = availableShots[UnityEngine.Random.Range(0, availableShots.Count)];
        _targets.Clear();
        _targets.Enqueue(selectedShot);
        OnTargetSelected(this, selectedShot.Target);
        OnTargetingEnd();
        Shoot();
    }

    public void HandleTargetClick(ShotStats target)
    {
        if (IsActive)
        {
            //Debug.Log("Shooter HandleTargetClick - " + gameObject.name + " - " + ActionName + " target: " + target.Unit.gameObject.name);
            while (_targets.Peek().Target != target.Target)
            {
                NextTarget();
            }
        }
    }

    public void HandleMouseOverTarget(ShotStats target)
    {

    }

    public void HandleMouseExitTarget(ShotStats target)
    {

    }

    override public void HandleConfirmClick()
    {
        Shoot();
    }

    protected void InvokeTargetingEnd()
    {
        OnTargetingEnd();
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        GetTargets();
        Available &= HasAvailableTargets();
        Available &= _weapon.Bullets > 0;
    }
}
