using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] bool _isAI;

    public bool IsActive { set; get; }

    NetworkMatchManager _matchManager;
    Pathfinder _pathfinder;
    GridManager _gridManager;
    GridAgent _gridAgent;

    public event Action OnActionComplete = delegate { };

    MyNetworkRoomManager _networkRoomManager;
    MyNetworkRoomManager NetworkRoomManager
    {
        get
        {
            if (_networkRoomManager != null) return _networkRoomManager;
            return _networkRoomManager = MyNetworkRoomManager.singleton as MyNetworkRoomManager;
        }
    }

    //

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Player OnStartClient");
        NetworkRoomManager.GamePlayers.Add(this);
        if (isServer)
            NetworkRoomManager.GamePlayerStart(this);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Player OnStopClient");
        NetworkRoomManager.GamePlayers.Remove(this);
    }

    private void Start()
    {
        _matchManager = NetworkMatchManager.Instance;
        _pathfinder = Pathfinder.Instance;
        _gridManager = GridManager.Instance;
    }

    public void Activate()
    {
        IsActive = true;
        if (isLocalPlayer)
        {
            if (_isAI)
            {
                if (_matchManager.CurrentCharacter.NumActions == 2)
                {
                    // pick a destination
                    GridEntity gridEntity = _matchManager.CurrentCharacter.GetComponent<GridEntity>();
                    _gridAgent = _matchManager.CurrentCharacter.GetComponent<GridAgent>();
                    Walker walker = _matchManager.CurrentCharacter.GetComponent<Walker>();
                    walker._NumMoves = _matchManager.CurrentCharacter.NumActions;
                    walker.OnActionComplete += HandleActionComplete;
                    GridNode _origin = gridEntity.CurrentNode;
                    int maxDistance = _gridAgent.WalkRange * walker._NumMoves;
                    float maxJumpUp = _gridAgent.MaxJumpUp;
                    float maxJumpDown = _gridAgent.MaxJumpDown;
                    _pathfinder.Initialize(_gridManager.GetGrid(), _origin, _origin, maxDistance, maxJumpUp, maxJumpDown, IsNodeAvailable);
                    List<GridNode> destinations = new List<GridNode>();
                    foreach (var node in _gridManager.GetGrid())
                    {
                        if (node.Distance < _gridAgent.WalkRange)
                        {
                            destinations.Add(node);
                        }
                    }
                    GridNode destination = destinations[UnityEngine.Random.Range(0, destinations.Count)];
                    Stack<GridNode> _path = new Stack<GridNode>();
                    _path = _pathfinder.GetPathTo(destination);
                    _gridAgent.BookedNode = destination;
                    destination.IsBooked = true;
                    walker.SetPath(_path, 1);
                }
                else if (_matchManager.CurrentCharacter.NumActions == 1)
                {
                    Shooter shooter = _matchManager.CurrentCharacter.Shooter;
                    shooter.GetTargets();
                    if (shooter.HasAvailableTargets())
                    {
                        shooter.OnActionComplete += HandleActionComplete;
                        shooter.ShootRandomTarget();
                    }
                    else
                    {
                        Skipper skipper = _matchManager.CurrentCharacter.GetComponent<Skipper>();
                        skipper.OnActionComplete += HandleActionComplete;
                        skipper.Skip();
                    }
                }
            }
            else
            {
                _matchManager.CurrentCharacter.Activate();
            }
        }
    }

    void HandleActionComplete(BattleAction battleAction)
    {
        //Debug.Log("HandleActionComplete");
        OnActionComplete();
        battleAction.OnActionComplete -= HandleActionComplete;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    bool IsNodeAvailable(GridNode node)
    {
        return _gridManager.IsNodeAvailable(node, _gridAgent);
    }
}
