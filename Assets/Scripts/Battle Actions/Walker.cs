using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Walker : BattleAction
{
    public static event Action<Walker, GridNode, GridNode> OnMove = delegate { };
    public static event Action<Walker, GridNode, GridNode> OnInMotion = delegate { };
    public static event Action<Walker, GridNode> OnDestinationReached = delegate { };

    [SerializeField] Movement _movement;

    int _numMoves;
    public int NumMoves { get { return _numMoves; } set { _numMoves = value; } }

    GridEntity _gridEntity;
    GridAgent _gridAgent;
    GridPathSelector _gridPathSelector;
    Stack<GridNode> _path = new Stack<GridNode>();

    public bool IsWalking { get; private set; }
    public bool IsRunning { get; private set; }

    // overwatch management 
    Health _health;

    bool _inMotion;

    private void Awake()
    {
        _gridPathSelector = GetComponent<GridPathSelector>();
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
        _health = GetComponent<Health>();
    }

    override protected void Update()
    {
        base.Update();
        if (_path.Count > 0)
        {
            if (!_inMotion && _movement.IsInMotion())
            {
                OnInMotion(this, _gridEntity.CurrentNode, _path.Peek());
                _inMotion = true;
            }
            if (_health.IsDead)
            {
                Stop();
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
                    IsRunning = false;
                    Deactivate();
                    OnDestinationReached(this, _gridEntity.CurrentNode);
                    InvokeActionComplete(this);
                }
            }
        }
    }

    public void Stop()
    {
        _gridEntity.CurrentNode = _path.Peek();
        _gridAgent.BookedNode.IsBooked = false;
        _gridAgent.BookedNode = null;
        _path.Clear();
        IsWalking = false;
        IsRunning = false;
        Deactivate();
        OnDestinationReached(this, _gridEntity.CurrentNode);
        InvokeActionComplete(this);
    }

    public void SetPath(Stack<GridNode> path, int cost)
    {
        //Debug.Log($"SetPath - ({name})");
        _path = new Stack<GridNode>(new Stack<GridNode>(path));
        Cost = cost;
        InvokeActionConfirmed(this);
        MoveToNextNode();
        IsWalking = true;
        IsRunning = Cost > 1;
    }

    void MoveToNextNode()
    {
        //Debug.Log("MoveToNextNode");
        GridNode nextStep = _path.Peek();
        Vector3 destination = nextStep.FloorPosition;
        bool leap = false;
        if (!GridNode.AreDiagonals(_gridEntity.CurrentNode, nextStep))
        {
            GridNode.Orientations orientation = GridNode.GetAdjacentDirection(_gridEntity.CurrentNode, nextStep);
            leap = _gridEntity.CurrentNode.HalfWalls[(int)orientation];
            //if (leap) Debug.Log($"Leap between {_gridEntity.CurrentNode.X},{_gridEntity.CurrentNode.Y},{_gridEntity.CurrentNode.Z} and {nextStep.X},{nextStep.Y},{nextStep.Z}.");
        }
        _inMotion = false;
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

    public override void Init(int numActions)
    {
        base.Init(numActions);
        _numMoves = numActions;
    }
}
