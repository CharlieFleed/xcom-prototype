using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Walker : BattleAction
{
    public static event Action<Walker, GridNode, GridNode> OnMove = delegate { };
    public static event Action<Walker, GridNode> OnDestinationReached = delegate { };

    [SerializeField] Movement _movement;    

    public int _NumMoves = 2;

    GridEntity _gridEntity;
    GridAgent _gridAgent;
    GridPathSelector _gridPathSelector;
    Stack<GridNode> _path = new Stack<GridNode>();

    public bool IsWalking { get; private set; }

    // overwatch management 
    Health _health;
    int _pauseLocks;

    private void Awake()
    {
        _gridPathSelector = GetComponent<GridPathSelector>();
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
        _health = GetComponent<Health>();
    }

    void Update()
    {

        if (_path.Count > 0 && _pauseLocks == 0)
        {
            if (_health.IsDead)
            {
                _gridEntity.CurrentNode = _path.Peek();
                _gridAgent.BookedNode.IsBooked = false;
                _gridAgent.BookedNode = null;
                _path.Clear();
                IsWalking = false;
                Deactivate();
                OnDestinationReached(this, _gridEntity.CurrentNode);
                InvokeActionComplete(this);
            }
            // if next step reached
            else if (_movement.IsAtDestination())
            {
                _gridEntity.CurrentNode = _path.Peek();
                _path.Pop();
                if (_path.Count > 0)
                {
                    MoveToNextNode();
                }
                else
                {
                    // destination reached
                    _gridAgent.BookedNode.IsBooked = false;
                    _gridAgent.BookedNode = null;
                    IsWalking = false;
                    Deactivate();
                    OnDestinationReached(this, _gridEntity.CurrentNode);
                    InvokeActionComplete(this);
                }
            }
        }
    }

    public void SetPath(Stack<GridNode> path, int cost)
    {
        _path = new Stack<GridNode>(new Stack<GridNode>(path));
        Cost = cost;
        InvokeActionConfirmed(this);
        MoveToNextNode();
        IsWalking = true;
    }

    void MoveToNextNode()
    {
        GridNode nextStep = _path.Peek();
        Vector3 destination = nextStep.FloorPosition;
        GridNode.Orientations orientation = GridManager.Instance.GetDirection(_gridEntity.CurrentNode, nextStep);
        bool leap = _gridEntity.CurrentNode.HalfWalls[(int)orientation];
        _movement.MoveToDestination(destination, leap);
        OnMove(this, _gridEntity.CurrentNode, nextStep);
    }

    override public void Activate()
    {
        //Debug.Log("Walker - Activate");
        base.Activate();
        _gridPathSelector.Activate();
    }

    override public void Cancel()
    {
        base.Cancel();
        _gridPathSelector.Cancel();
    }

    public void Pause()
    {
        //Debug.Log("Pause");
        _pauseLocks++;
    }

    public void Resume()
    {
        //Debug.Log("Resume");
        _pauseLocks--;
    }
}
