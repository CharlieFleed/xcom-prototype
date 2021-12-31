using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;

public class UnitLocalController : NetworkBehaviour
{
    #region Fields

    public static event Action<UnitLocalController> OnUnitLocalControllerAdded = delegate { };
    public static event Action<UnitLocalController> OnUnitLocalControllerRemoved = delegate { };

    Walker _walker;
    BattleAction[] _battleActions;
    ActionsController _actionsController;
    bool _isActionActive;

    private bool _isActive;
    public bool IsActive { get { return _isActive; } private set { _isActive = value; OnActiveChanged(this, _isActive); } }
    public static event Action<UnitLocalController, bool> OnActiveChanged = delegate { };

    public event Action<BattleAction> OnActionAdded = delegate { };
    public event Action OnActionsCleared = delegate { };

    public event Action<BattleAction> OnActionActivated = delegate { };
    public event Action OnActionConfirmed = delegate { };

    public event Action OnPass = delegate { };
    public event Action OnEndTurn = delegate { };
    public event Action OnPause = delegate { };

    public event Action<ShotStats> OnMouseOverTarget = delegate { };
    public event Action<ShotStats> OnMouseExitTarget = delegate { };

    public Weapon Weapon
    {
        get
        {
            if (Shooter != null)
                return Shooter.Weapon;
            return null;
        }
    }

    Shooter Shooter
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

    InputCache _input = new InputCache();

    #endregion

    private void Awake()
    {
        _actionsController = GetComponent<ActionsController>();
        _battleActions = GetComponents<BattleAction>();
        _walker = GetComponent<Walker>();
        //
        foreach (var battleAction in _battleActions)
        {
            battleAction.OnActionCancelled += HandleActionCancelled;
            battleAction.OnActionConfirmed += HandleActionConfirmed;
            battleAction.OnActionComplete += HandleActionComplete;
        }
    }

    private void Start()
    {
        //Debug.Log($"Unit Start for {name}.");
        OnUnitLocalControllerAdded(this);
    }

    private void OnDestroy()
    {
        OnUnitLocalControllerRemoved(this);
        //
        foreach (var battleAction in _battleActions)
        {
            battleAction.OnActionCancelled -= HandleActionCancelled;
            battleAction.OnActionConfirmed -= HandleActionConfirmed;
            battleAction.OnActionComplete -= HandleActionComplete;
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
                for (int i = 1; i < _battleActions.Length; i++) // NOTE: this assumes Walker is the first action.
                {
                    if (_input.GetKeyDown(KeyCode.Alpha1 + i -1))
                    {
                        if (!_battleActions[i].IsActive && _battleActions[i].Available)
                        {
                            ActivateBattleAction(_battleActions[i]);
                        }
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
        foreach (BattleAction battleAction in _battleActions)
        {
            if (!(battleAction is Walker))
                OnActionAdded(battleAction);
        }
        _walker.Activate();
        ShowMainShooterTargets();
    }

    void InitActions()
    {
        foreach (var battleAction in _battleActions)
        {
            battleAction.Init(_actionsController.NumActions);
        }
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
    void ShowMainShooterTargets()
    {
        //Debug.Log("Unit Show Targets");
        foreach (var battleAction in _battleActions)
        {
            if (battleAction is Shooter)
            {
                ((Shooter)battleAction).ShowTargets();
                break;
            }
        }
    }

    /// <summary>
    /// Hide targets for the first shooter found.
    /// </summary>
    void HideMainShooterTargets()
    {
        //Debug.Log("Unit Hide Targets");
        foreach (var battleAction in _battleActions)
        {
            if (battleAction is Shooter)
            {
                ((Shooter)battleAction).HideTargets();
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
        foreach (var battleAction in _battleActions)
        {
            if (!(battleAction is Walker) && battleAction.IsActive)
            {
                battleAction.Cancel();
            }
        }
    }

    void Cancel()
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
        //Debug.Log("Unit - HandleActionConfirmed");
        if (_walker.IsActive)
        {
            HideMainShooterTargets();
        }
        OnActionsCleared();
        OnActionConfirmed();
    }

    void HandleActionComplete(BattleAction battleAction)
    {
        //Debug.Log("Unit - HandleActionComplete");
        _isActionActive = false;
        if (IsActive)
        {
            Deactivate();
        }
    }

    void HandleActionCancelled(BattleAction battleAction)
    {
        //Debug.Log("Unit - HandleActionCancelled");
        if (!(battleAction is Walker))
        {
            _isActionActive = false;
            _walker.Activate();
            ShowMainShooterTargets();
        }
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
                if (battleAction is Shooter && battleAction.Available)
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