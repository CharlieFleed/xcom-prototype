using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewMode {Single, Team, Always};
    
public class Viewer : MonoBehaviour
{
    public static ViewMode ViewMode = ViewMode.Team;
    /// <summary>
    /// Range in tiles.
    /// </summary>
    [SerializeField] int _range = 20;

    public List<Viewer> SeenByList { set; get; }

    bool _isVisible;
    public bool IsVisible { get { return _isVisible; } private set { if (_isVisible != value) { _isVisible = value; OnVisibleChanged(this, _isVisible); } } }

    /// <summary>
    /// Fired only on actual changes.
    /// </summary>
    public static event Action<Viewer, bool> OnVisibleChanged = delegate { };

    Health _health;
    TeamMember _teamMember;
    GridEntity _gridEntity;
    Renderer[] _renderers;


    // cached values for performance
    public TeamMember TeamMember { get { return _teamMember; } }
    public Health Health { get { return _health; } }
    public int Range { get { return _range; } }

    private void Awake()
    {
        SeenByList = new List<Viewer>();
        _health = GetComponent<Health>();
        _teamMember = GetComponent<TeamMember>();
        _gridEntity = GetComponent<GridEntity>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnEnable()
    {
        NetworkMatchManager.Instance.OnBeforeTurnBegin += HandleNetworkMatchManager_OnBeforeTurnBegin;
        Walker.OnMove += HandleWalker_OnMove;
        Walker.OnDestinationReached += HandleWalker_OnDestinationReached;
    }

    private void OnDisable()
    {
        NetworkMatchManager.Instance.OnBeforeTurnBegin -= HandleNetworkMatchManager_OnBeforeTurnBegin;
        Walker.OnMove -= HandleWalker_OnMove;
        Walker.OnDestinationReached -= HandleWalker_OnDestinationReached;
    }

    private void HandleNetworkMatchManager_OnBeforeTurnBegin()
    {
        UpdateViewers();
        UpdateVisibility();
    }

    void HandleWalker_OnMove(Walker walker, GridNode origin, GridNode gridNode)
    {
        UpdateViewers();
        UpdateVisibility();
    }

    private void HandleWalker_OnDestinationReached(Walker arg1, GridNode arg2)
    {
        UpdateViewers();
        UpdateVisibility();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVisibility();   
        foreach (var renderer in _renderers)
        {
            renderer.enabled = _isVisible;
        }
    }

    void UpdateViewers()
    {
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(_teamMember);
        SeenByList.Clear();
        foreach (var enemy in enemies)
        {
            Viewer enemyViewer = enemy.GetComponent<Viewer>();
            if (GridCoverManager.Instance.LineOfSight(_gridEntity, enemy, out Ray ray, out float rayLength, new List<GridNode[]>()))
            {
                if (!enemyViewer.Health.IsDead)
                {
                    SeenByList.Add(enemyViewer);
                }
            }
        }
    }

    void UpdateVisibility()
    {
        switch (ViewMode)
        {
            case ViewMode.Single:
                if (NetworkMatchManager.Instance.CurrentTeamMember != null)
                {
                    if (NetworkMatchManager.Instance.CurrentTeamMember.Team.Owner.isLocalPlayer)
                    {
                        IsVisible = _teamMember.Team.Owner.isLocalPlayer || SeenByList.Contains(NetworkMatchManager.Instance.CurrentTeamMember.GetComponent<Viewer>());
                    }
                    else
                    {
                        IsVisible = _teamMember.Team.Owner.isLocalPlayer || IsSeenByLocalPlayer();
                    }
                }
                else
                {
                    IsVisible = _teamMember.Team.Owner.isLocalPlayer || IsSeenByLocalPlayer();
                }
                break;
            case ViewMode.Team:
                IsVisible = (SeenByList.Count > 0) || (_teamMember.Team.Owner.isLocalPlayer && !_teamMember.Team.IsAI);
                break;
            case ViewMode.Always:
                IsVisible = true;
                break;
            default:
                break;
        }
    }

    bool IsSeenByLocalPlayer()
    {
        foreach (var viewer in SeenByList)
        {
            if (viewer.TeamMember.Team.Owner.isLocalPlayer)
            {
                return true;
            }
        }
        return false;
    }
}
