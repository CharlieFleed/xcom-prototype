using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] bool _isAI;

    public bool IsActive { set; get; }

    MatchManager _matchManager;
    Pathfinder _pathfinder;
    GridManager _gridManager;

    GridAgent _gridAgent;

    public event Action OnActionComplete = delegate { };

    private void Start()
    {
        _matchManager = MatchManager.Instance;
        _pathfinder = Pathfinder.Instance;
        _gridManager = GridManager.Instance;
    }

    public void Activate()
    {
        IsActive = true;
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
                _matchManager.CurrentCharacter.MainShooter().GetTargets();
                if (_matchManager.CurrentCharacter.MainShooter().HasAvailableTargets())
                {
                    _matchManager.CurrentCharacter.MainShooter().OnActionComplete += HandleActionComplete;
                    _matchManager.CurrentCharacter.MainShooter().ShootRandomTarget();
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
