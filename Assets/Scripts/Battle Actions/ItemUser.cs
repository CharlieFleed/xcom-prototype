using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;

public class ItemUser : BattleAction
{
    #region Fields

    public static event Action<ItemUser> OnItemUserAdded = delegate { };
    public static event Action<ItemUser> OnItemUserRemoved = delegate { };

    public event Action<ShotStats> OnTargetAdded = delegate { };
    public event Action OnHideTargets = delegate { };

    public event Action<ItemUser, GridEntity> OnTargetSelected = delegate { };
    public event Action OnTargetingEnd = delegate { };

    public event Action<Item> OnUsed = delegate { };
    public event Action<ItemUser> OnUse = delegate { };

    protected Queue<ShotStats> _targets = new Queue<ShotStats>();

    [SerializeField] protected Item _item;
    public Item Item { get { return _item; } set { _item = value; } }

    public override string ActionName { get { return _item.Name; } }
    public override string ConfirmText { get { return "Use " + _item.Name; } }

    public static event Action<ShotStats> OnShotSelected = delegate { };

    GridEntity _gridEntity;
    GridAgent _gridAgent;
    Pathfinder _pathfinder;
    GridManager _gridManager;
    List<GridNode> _targetNodes = new List<GridNode>();

    #endregion

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
    }

    private void Start()
    {
        _pathfinder = Pathfinder.Instance;
        _gridManager = GridManager.Instance;
    }

    private void OnEnable()
    {
        OnItemUserAdded(this);
    }

    private void OnDisable()
    {
        OnItemUserRemoved(this);
    }

    private void Update()
    {
        if (IsActive)
        {
            _input.Update();
        }
    }
    // Update is called once per frame
    void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (_input.GetKeyDown(KeyCode.Tab))
            {
                NextTarget();
            }
            if (_input.GetKeyDown(KeyCode.Return))
            {
                Use();
            }
            if (_input.GetKeyDown(KeyCode.Escape) || _input.GetMouseButtonDown(1))
            {
                Cancel();
            }
            UpdateHighlight();
        }
        _input.Clear();
    }

    protected virtual void Use()
    {
        if (_targets.Count > 0 && _targets.Peek().Available)
        {
            HideTargets();
            OnTargetingEnd();
            CmdUse(_targets.Peek().Target.gameObject);
        }
    }

    [Command]
    protected void CmdUse(GameObject target)
    {
        RpcUse(target);
    }

    [ClientRpc]
    protected virtual void RpcUse(GameObject target)
    {
        Debug.Log(" ItemUser RpcUse");
        GetTargets();
        ShotStats shotStats = null;
        foreach (var shot in _targets)
        {
            if (shot.Target == target.GetComponent<GridEntity>())
            {
                shotStats = shot;
            }
        }
        OnTargetSelected(this, shotStats.Target);
        OnTargetingEnd();
        Debug.Log($"Use {_item.name} on {shotStats.Target.name}");
        BattleEventItemUse itemUseEvent = new BattleEventItemUse(this, shotStats);
        NetworkMatchManager.Instance.AddBattleEvent(itemUseEvent, true);
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }

    public void Used()
    {
        OnUsed(_item);
    }

    public void DoUse()
    {
        _item.Use();
        OnUse(this);
        Invoke("Used", 1f);
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
        //Debug.Log("ItemUser - Activate");
        base.Activate();
        ShowTargets();
        StartTargetSelection();

    }

    void UpdateHighlight()
    {
        foreach (var node in _targetNodes)
        {
            GridHighlightManager.Instance.HighlightNode(node);
        }
    }

    override public void Cancel()
    {
        //Debug.Log("ItemUser - Cancel");
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
        List<GridEntity> friends = NetworkMatchManager.Instance.GetFriendsAs<GridEntity>(GetComponent<Unit>());
        foreach (var shotStats in GridCoverManager.Instance.GetShotStats(this, friends))
        {
            if (Vector3.Distance(transform.position, shotStats.Target.CurrentNode.FloorPosition) <= _item.Range)
            {
                shotStats.Available &= _item.IsApplicable(shotStats.Target);
                shotStats.HitChance = 100;
                shotStats.CritChance = 0;
                shotStats.BaseDamage = 0;
                shotStats.MaxDamage = 0;
                shotStats.Friendly = true;
                _targets.Enqueue(shotStats);
            }
        }
    }

    public void ShowTargets()
    {
        foreach (var target in _targets)
        {
            OnTargetAdded(target);
            Debug.Log("ItemUser TargetAdded");
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

    public void HandleTargetClick(ShotStats target)
    {
        if (IsActive)
        {
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
        Use();
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
        Available &= _item.Uses > 0;
        //
        _targetNodes.Clear();
        foreach (var node in _gridManager.GetGrid().Nodes())
        {
            if (Vector3.Distance(transform.position, node.FloorPosition) <= _item.Range)
            {
                _targetNodes.Add(node);
            }
        }

        //GridNode origin = _gridEntity.CurrentNode;
        //float maxJumpUp = _gridAgent.MaxJumpUp;
        //float maxJumpDown = _gridAgent.MaxJumpDown;
        //_pathfinder.Initialize(_gridManager.GetGrid(), origin, origin, _item.Range, maxJumpUp, maxJumpDown, (node) => { return true; }, false);
        //_targetNodes = _pathfinder.GetNodes(_item.Range);
    }
}
