using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActionsController : MonoBehaviour
{
    #region Events

    public static event Action<ActionsController> OnActionsControllerAdded = delegate { };
    public static event Action<ActionsController> OnActionsControllerRemoved = delegate { };

    public event Action<Unit> OnActionComplete = delegate { };
    public event Action<int> OnNumActionsChanged = delegate { };

    #endregion

    #region Fields

    BattleAction[] _battleActions;
    Unit _unit;

    private int _numActions;
    public int NumActions { get { return _numActions; } set { _numActions = value; OnNumActionsChanged(_numActions); } }

    #endregion

    private void Awake()
    {
        _battleActions = GetComponents<BattleAction>();
        _unit = GetComponent<Unit>();

        foreach (var battleAction in _battleActions)
        {
            battleAction.OnActionComplete += HandleActionComplete;
            battleAction.OnActionConfirmed += HandleActionConfirmed;
        }
    }

    private void Start()
    {
        OnActionsControllerAdded(this);
    }

    void HandleActionComplete(BattleAction battleAction)
    {
        OnActionComplete(_unit);
    }

    void HandleActionConfirmed(BattleAction battleAction)
    {
        if (battleAction.EndsTurn)
        {
            NumActions = 0;
        }
        else
        {
            NumActions -= battleAction.Cost;
        }
    }

    private void OnDestroy()
    {
        OnActionsControllerRemoved(this);
        foreach (var battleAction in _battleActions)
        {
            battleAction.OnActionComplete -= HandleActionComplete;
            battleAction.OnActionConfirmed -= HandleActionConfirmed;
        }
    }

    public void StartTurn()
    {
        NumActions = 2;
        InitActions();
    }

    void InitActions()
    {
        foreach (var battleAction in _battleActions)
        {
            battleAction.Init(NumActions);
        }
    }
}
