using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;

public class Unit : NetworkBehaviour
{
    #region Fields

    public static event Action<Unit> OnUnitAdded = delegate { };
    public static event Action<Unit> OnUnitRemoved = delegate { };

    [SerializeField] Walker _walker;
    [SerializeField] List<BattleAction> _battleActions;

    private bool _isActive;
    public bool IsActive { get { return _isActive; } private set { _isActive = value; OnActiveChanged(this, _isActive); } }
    public static event Action<Unit, bool> OnActiveChanged = delegate { };

    private int _numActions;
    public int NumActions { get { return _numActions; } private set { _numActions = value; OnNumActionsChanged(_numActions); } }
    public event Action<int> OnNumActionsChanged = delegate { };

    public event Action<BattleAction> OnActionAdded = delegate { };
    public event Action OnActionsCleared = delegate { };

    public event Action<BattleAction> OnActionActivated = delegate { };
    public event Action OnActionConfirmed = delegate { };
    public event Action<Unit> OnActionComplete = delegate { };
    public event Action OnPass = delegate { };
    public event Action OnEndTurn = delegate { };
    public event Action OnPause = delegate { };

    bool _isActionActive;

    public event Action<ShotStats> OnMouseOverTarget = delegate { };
    public event Action<ShotStats> OnMouseExitTarget = delegate { };

    public Team Team { set; get; } = null;

    public Weapon Weapon
    {
        get
        {
            if (Shooter != null)
                return Shooter.Weapon;
            return null;
        }
    }

    public Shooter Shooter
    {
        get
        {
            foreach (var item in _battleActions)
            {
                if (item is Shooter)
                {
                    return (Shooter)item;
                }
            }
            return null;
        }
    }

    public bool Started { private set; get; }

    InputCache _input = new InputCache();

    #endregion

    private void Start()
    {
        //Debug.Log($"Unit Start for {name}.");
        OnUnitAdded(this);
        Started = true;
    }

    private void OnDisable()
    {
        OnUnitRemoved(this);
        if (Team != null)
        {
            Team.RemoveUnit(this);
        }
    }

    private void Awake()
    {
        _walker.OnActionComplete += HandleActionComplete;
        _walker.OnActionConfirmed += HandleActionConfirmed;
        //
        foreach (var battleAction in _battleActions)
        {
            battleAction.OnActionCancelled += HandleActionCancelled;
            battleAction.OnActionConfirmed += HandleActionConfirmed;
            battleAction.OnActionComplete += HandleActionComplete;
        }
    }

    private void Update()
    {
        _input.Update();
    }    

    private void LateUpdate()
    {
        if (IsActive)
        {
            if (!_isActionActive)
            {
                if (_input.GetKeyDown(KeyCode.Tab))
                {
                    Cancel();
                    CmdPass();
                }
                else if (_input.GetKeyDown(KeyCode.Escape))
                {
                    OnPause();
                }
            }
            if (!_walker.IsWalking)
            {
                if (_input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (!_battleActions[0].IsActive && _battleActions[0].Available)
                    {
                        ActivateBattleAction(_battleActions[0]);
                    }
                }
                if (_input.GetKeyDown(KeyCode.Alpha2) && _battleActions[1].Available)
                {
                    if (!_battleActions[1].IsActive)
                    {
                        ActivateBattleAction(_battleActions[1]);
                    }
                }
            }
        }
        _input.Clear();
    }

    [Command]
    void CmdPass()
    {
        RpcPass();
    }

    [ClientRpc]
    void RpcPass()
    {
        OnPass();
    }

    [Command]
    void CmdEndTurn()
    {
        RpcEndTurn();
    }

    [ClientRpc]
    void RpcEndTurn()
    {
        OnEndTurn();
    }

    public void StartTurn()
    {
        NumActions = 2;
        InitActions();
    }

    public void InitActions()
    {
        foreach (var battleAction in _battleActions)
        {
            battleAction.Init(NumActions);
        }
    }

    /// <summary>
    /// Only local units are activated in multiplayer.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        if (_walker.IsActive)
        {
            return;
        }
        else
        {
            StartCoroutine(ActivateControls());            
        }
    }

    System.Collections.IEnumerator ActivateControls()
    {
        yield return new WaitForSeconds(0.5f);
        InitActions();
        foreach (var battleAction in _battleActions)
        {
            OnActionAdded(battleAction);
        }
        _walker._NumMoves = NumActions;
        _walker.Activate();
        ShowMainShooterTargets();
    }

    void Deactivate()
    {
        IsActive = false;
        HideMainShooterTargets();
        OnActionsCleared();
        StopAllCoroutines();
    }

    /// <summary>
    /// Show targets for the first shooter found.
    /// </summary>
    private void ShowMainShooterTargets()
    {
        //Debug.Log("Unit Show Targets");
        foreach (var item in _battleActions)
        {
            if (item is Shooter)
            {
                ((Shooter)item).ShowTargets();
                break;
            }
        }
    }

    /// <summary>
    /// Hide targets for the first shooter found.
    /// </summary>
    private void HideMainShooterTargets()
    {
        //Debug.Log("Unit Hide Targets");
        foreach (var item in _battleActions)
        {
            if (item is Shooter)
            {
                ((Shooter)item).HideTargets();
                break;
            }
        }
    }

    public void SelectAction(BattleAction battleAction)
    {
        if (!battleAction.IsActive && battleAction.Available)
        {
            ActivateBattleAction(battleAction);
        }
    }

    void ActivateBattleAction(BattleAction battleAction)
    {
        //Debug.Log("Unit - ActivateBattleAction");
        CancelActiveBattleAction();
        if (_walker.IsActive)
        {
            _walker.Cancel();
            HideMainShooterTargets();
        }
        battleAction.Activate();
        _isActionActive = true;
        OnActionActivated(battleAction);
    }

    void CancelActiveBattleAction()
    {
        foreach (var item in _battleActions)
        {
            if (item.IsActive)
            {
                item.Cancel();
            }
        }
    }

    public void Cancel()
    {
        Deactivate();
        if (!_walker.IsWalking)
        {
            _walker.Cancel();
        }
    }

    #region Event Handlers

    void HandleActionConfirmed(BattleAction battleAction)
    {
        //Debug.Log("HandleActionConfirmed");
        if (_walker.IsActive)
        {
            HideMainShooterTargets();
        }
        if (battleAction.EndsTurn)
        {
            NumActions = 0;
        }
        else
        {
            NumActions -= battleAction.Cost;
        }
        OnActionsCleared();
        OnActionConfirmed();
    }

    void HandleActionComplete(BattleAction battleAction)
    {
        _isActionActive = false;
        if (IsActive)
        {
            Deactivate();
        }
        OnActionComplete(this);
    }

    void HandleActionCancelled(BattleAction battleAction)
    {
        //Debug.Log("Unit - HandleActionCancelled");
        _isActionActive = false;
        _walker.Activate();
        ShowMainShooterTargets();
    }

    #endregion

    #region GUI Event Handlers

    public void HandleTargetClick(ShotStats target)
    {
        if (IsActive && !_isActionActive)
        {
            //Debug.Log("Unit HandleTargetClick - " + gameObject.name + " target: " + target.Unit.gameObject.name);
            foreach (var battleAction in _battleActions)
            {
                if (battleAction is Shooter && battleAction.Available) // TODO: check if the target is available for this shooter?
                {
                    ActivateBattleAction(battleAction);
                    ((Shooter)battleAction).SelectTarget(target);
                    target.Target.IsSoftTargeted = false;
                    break;
                }
            }
        }
    }

    public void HandleMouseOverTarget(ShotStats target)
    {
        if (IsActive && !_isActionActive)
        {
            //Debug.Log("Unit HandleMouseOverTarget - " + gameObject.name + " target: " + target.Unit.gameObject.name);
            target.Target.IsSoftTargeted = true;
            OnMouseOverTarget(target);
        }
    }

    public void HandleMouseExitTarget(ShotStats target)
    {
        if (IsActive && !_isActionActive)
        {
            //Debug.Log("Unit HandleMouseExitTarget - " + gameObject.name + " target: " + target.Unit.gameObject.name);
            target.Target.IsSoftTargeted = false;
            OnMouseExitTarget(target);
        }
    }

    public void HandlePass()
    {
        if (IsActive && !_isActionActive)
        {
            Cancel();
            CmdPass();
        }
    }

    public void HandleEndTurn()
    {
        if (IsActive && !_isActionActive)
        {
            Cancel();
            CmdEndTurn();
        }
    }

    #endregion
}