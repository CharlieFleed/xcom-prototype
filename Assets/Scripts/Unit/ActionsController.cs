﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActionsController : MonoBehaviour
{
    public static event Action<ActionsController> OnActionsControllerAdded = delegate { };
    public static event Action<ActionsController> OnActionsControllerRemoved = delegate { };

    public event Action<TeamMember> OnActionComplete = delegate { };

    BattleAction[] _battleActions;
    TeamMember _teamMember;

    private int _numActions;
    public int NumActions { get { return _numActions; } private set { _numActions = value; OnNumActionsChanged(_numActions); } }
    public event Action<int> OnNumActionsChanged = delegate { };

    private void Awake()
    {
        _battleActions = GetComponents<BattleAction>();
        _teamMember = GetComponent<TeamMember>();

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
        OnActionComplete(_teamMember);
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
