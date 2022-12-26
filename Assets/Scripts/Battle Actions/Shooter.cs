using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;
using Mirror;
using System.Linq;

public class Shooter : BattleAction
{
    #region Fields

    public static event Action<Shooter> OnShooterAdded = delegate { };
    public static event Action<Shooter> OnShooterRemoved = delegate { };

    public event Action<ShotStats> OnTargetAdded = delegate { };
    public event Action OnHideTargets = delegate { };

    public event Action<Shooter, GridEntity> OnTargetSelected = delegate { };
    public event Action OnTargetingEnd = delegate { };

    protected void InvokeOnTargetSelected(Shooter shooter, GridEntity gridEntity)
    {
        OnTargetSelected(shooter, gridEntity);
    }
    protected void InvokeOnTargetingEnd()
    {
        OnTargetingEnd.Invoke();
    }

    public event Action OnShot = delegate { };
    public event Action<Shooter> OnShoot = delegate { };

    public static event Action<ShotStats> OnShotSelected = delegate { };

    [SerializeField] protected Weapon _weapon;
    GridEntity _gridEntity;
    protected Queue<ShotStats> _shots = new Queue<ShotStats>();

    public Weapon Weapon { get { return _weapon; } set { _weapon = value; } }
    public override string ActionName { get { return _weapon.Name; } }
    public override string ConfirmText { get { return "Fire " + _weapon.Name; } }

    public bool IsShooting { private set; get; }

    #endregion

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
    }

    private void OnEnable()
    {
        OnShooterAdded(this);
    }

    private void OnDisable()
    {
        OnShooterRemoved(this);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (IsActive)
        {
            if (_input.GetKeyDown(KeyCode.Tab))
            {
                NextTarget();
            }
            if (_input.GetKeyDown(KeyCode.Return))
            {
                Shoot();
            }
            if (_input.GetKeyDown(KeyCode.Escape) || _input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
        _input.Clear();
    }

    protected virtual void Shoot()
    {
        if (_shots.Count > 0 && _shots.Peek().Available)
        {
            IsShooting = true;
            HideTargets();
            OnTargetingEnd();
            CmdShoot(_shots.Peek().Target.gameObject);
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
        //Debug.Log($"{name} Shooter RpcShoot");
        UpdateShots();
        ShotStats shotStats = null;
        foreach (var shot in _shots)
        {
            if (shot.Target == target.GetComponent<GridEntity>())
            {
                shotStats = shot;
            }
        }
        //if (shotStats == null) Debug.Log($"PANIC! target {target.name} not found!");
        OnTargetSelected(this, shotStats.Target);
        OnTargetingEnd();
        //Debug.Log($"Shoot {shotStats.Target.name}");
        BattleEventShot shotEvent = new BattleEventShot(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(shotEvent, 2, BattleEvent.CreateNewGroup);
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    public void Shot()
    {
        OnShot();
        _weapon.Shot();
        IsShooting = false;
    }

    public void DoShoot()
    {
        _weapon.Shoot();
        OnShoot(this);
    }

    void NextTarget()
    {
        if (_shots.Count > 0)
        {
            // unselect current target
            _shots.Peek().Target.IsTargeted = false;
            // rotate targets
            ShotStats target = _shots.Dequeue();
            _shots.Enqueue(target);
            // select new target
            _shots.Peek().Target.IsTargeted = true;
            OnTargetSelected(this, _shots.Peek().Target);
            OnShotSelected(_shots.Peek());
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
        if (_shots.Count > 0)
        {
            _shots.Peek().Target.IsTargeted = false;
        }
        HideTargets();
        OnTargetingEnd();
        base.Cancel();        
    }

    void StartTargetSelection()
    {
        if (_shots.Count > 0)
        {
            _shots.Peek().Target.IsTargeted = true;
            OnTargetSelected(this, _shots.Peek().Target);
            OnShotSelected(_shots.Peek());
        }
    }

    public void UpdateShots()
    {
        _shots.Clear();
        // enemies
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(GetComponent<Unit>());
        GetShotStats(_shots, enemies, _gridEntity.CurrentNode);
        // other entities
        List<GridEntity> gridEntities = NetworkMatchManager.Instance.GetGridEntities();
        // Add map entities
        foreach (var gridEntity in gridEntities)
        {
            GridAgent gridAgent = gridEntity.GetComponent<GridAgent>();
            if (gridAgent == null)
            {
                if (GridCoverManager.Instance.LineOfSight(_gridEntity, gridEntity, out Ray ray, out float rayLength, new List<GridNode[]>()))
                {
                    ShotStats shotStats = new ShotStats();
                    shotStats.Target = gridEntity;
                    shotStats.Available = Vector3.Distance(transform.position, gridEntity.transform.position) <= _weapon.Range;
                    ShotStatsHelper.UpdateMapEntityShotStats(shotStats, this);
                    _shots.Enqueue(shotStats);
                }
            }
        }
    }

    private void GetShotStats(Queue<ShotStats> shots, List<GridEntity> enemies, GridNode currentNode)
    {
        foreach (var shot in GridCoverManager.Instance.GetShotStats(_gridEntity, currentNode, enemies))
        {
            shot.Available = Vector3.Distance(currentNode.FloorPosition, shot.Target.CurrentNode.FloorPosition) <= _weapon.Range;
            ShotStatsHelper.UpdateShotStats(shot, currentNode, this, ShotStatsHelper.RegularShot);
            shots.Enqueue(shot);
        }
    }

    public Queue<ShotStats> GetShotsFromPosition(GridNode position)
    {
        Queue<ShotStats> shots = new Queue<ShotStats>();
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(GetComponent<Unit>());
        GetShotStats(shots, enemies, position);
        return shots;
    }

    public void ShowTargets()
    {
        //Debug.Log($"Shooter - ShowTargets. #targets: {_targets.Count}.");
        foreach (var target in _shots)
        {
            OnTargetAdded(target);
        }
    }

    public void HideTargets()
    {
        foreach (var target in _shots)
        {
            target.Target.IsTargeted = false;
        }
        OnHideTargets();
    }

    public bool HasAvailableTargets()
    {
        foreach (var target in _shots)
        {
            if (target.Available)
                return true;
        }
        return false;
    }

    public List<ShotStats> GetTargets()
    {
        return _shots.ToList();
    }

    public void SelectTarget(ShotStats target)
    {
        if (_shots.Count > 0)
        {
            while (_shots.Peek().Target != target.Target)
            {
                NextTarget();
            }
        }
    }

    public void ShootRandomTarget()
    {
        List<ShotStats> availableShots = new List<ShotStats>();
        foreach (var shot in _shots)
        {
            if (shot.Available)
                availableShots.Add(shot);
        }
        ShotStats selectedShot = availableShots[UnityEngine.Random.Range(0, availableShots.Count)];
        _shots.Clear();
        _shots.Enqueue(selectedShot);
        OnTargetSelected(this, selectedShot.Target);
        OnTargetingEnd();
        Shoot();
    }

    public void ShootTarget(ShotStats shot)
    {
        _shots.Clear();
        _shots.Enqueue(shot);
        OnTargetSelected(this, shot.Target);
        OnTargetingEnd();
        Shoot();
    }

    public void HandleTargetClick(ShotStats target)
    {
        if (IsActive)
        {
            //Debug.Log("Shooter HandleTargetClick - " + gameObject.name + " - " + ActionName + " target: " + target.Unit.gameObject.name);
            while (_shots.Peek().Target != target.Target)
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
        UpdateShots();
        Available &= HasAvailableTargets();
        Available &= _weapon.Bullets > 0;
    }
}
