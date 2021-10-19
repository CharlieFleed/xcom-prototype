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

    bool _hunkering;
    public bool Hunkering { get { return _hunkering; } set { _hunkering = value; OnHunkeringChanged(_hunkering); } }
    public event Action<bool> OnHunkeringChanged = delegate { };

    private void Start()
    {
        OnGridAgentAdded(this);
        UpdateCover();
    }

    private void OnDisable()
    {
        OnGridAgentRemoved(this);
    }

    private void Awake()
    {
        Walker.OnDestinationReached += HandleWalker_OnDestinationReached;
        MatchManager.Instance.OnNewTurn += HandleMatchManager_OnNewTurn;
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
        List<GridEntity> enemies = MatchManager.Instance.GetEnemiesAs<GridEntity>(GetComponent<Character>());
        Cover = GridCoverManager.Instance.GetCover(GetComponent<GridEntity>(), enemies);
    }

    private void OnDestroy()
    {
        Walker.OnDestinationReached -= HandleWalker_OnDestinationReached;
        MatchManager.Instance.OnNewTurn -= HandleMatchManager_OnNewTurn;
    }
}
