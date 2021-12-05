using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GridAgent : MonoBehaviour
{
    public static event Action<GridAgent> OnGridAgentAdded = delegate { };
    public static event Action<GridAgent> OnGridAgentRemoved = delegate { };



    [SerializeField] int _walkRange = 10;
    [SerializeField] float _maxJumpUp = 5;
    [SerializeField] float _maxJumpDown = 5;

    public int WalkRange { get { return _walkRange; } }
    public float MaxJumpUp { get { return _maxJumpUp; } }
    public float MaxJumpDown { get { return _maxJumpDown; } }

    public GridNode BookedNode { get; set; }

    int _cover;
    public int Cover { get { return _cover; } private set { _cover = value; OnCoverChanged(_cover); } }
    public event Action<int> OnCoverChanged = delegate { };

    private void Start()
    {
        OnGridAgentAdded(this);
        UpdateCover();
    }

    private void OnEnable()
    {
        Walker.OnDestinationReached += HandleWalker_OnDestinationReached;
        NetworkMatchManager.Instance.OnTurnBegin += HandleMatchManager_OnNewTurn;
    }

    private void OnDisable()
    {
        OnGridAgentRemoved(this);
        Walker.OnDestinationReached -= HandleWalker_OnDestinationReached;
        NetworkMatchManager.Instance.OnTurnBegin -= HandleMatchManager_OnNewTurn;
    }

    void HandleMatchManager_OnNewTurn()
    {
        UpdateCover();
    }

    void HandleWalker_OnDestinationReached(Walker arg1, GridNode arg2)
    {
        UpdateCover();
    }

    void UpdateCover()
    {
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(GetComponent<TeamMember>());
        Cover = GridCoverManager.Instance.GetCover(GetComponent<GridEntity>(), enemies);
    }
}
