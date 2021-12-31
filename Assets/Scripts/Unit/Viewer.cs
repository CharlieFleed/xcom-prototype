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
    public List<Viewer> SeeList { set; get; }

    bool _isVisible;
    public bool IsVisible { get { return _isVisible; } private set { if (_isVisible != value) { _isVisible = value; OnVisibleChanged(this, _isVisible); } } }

    /// <summary>
    /// Fired only on actual changes.
    /// </summary>
    public static event Action<Viewer, bool> OnVisibleChanged = delegate { };

    Health _health;
    Unit _unit;
    GridEntity _gridEntity;
    Renderer[] _renderers;


    // cached values for performance
    public Unit Unit { get { return _unit; } }
    public Health Health { get { return _health; } }
    public int Range { get { return _range; } }

    private void Awake()
    {
        SeenByList = new List<Viewer>();
        SeeList = new List<Viewer>();
        _health = GetComponent<Health>();
        _unit = GetComponent<Unit>();
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
        // Update before the camera director
        UpdateViewers();
        UpdateVisibility();
    }

    void HandleWalker_OnMove(Walker walker, GridNode origin, GridNode destination)
    {
        UpdateViewers();
        UpdateVisibility(); // TODO: is this necessary? Is the whole method necessary?
    }

    private void HandleWalker_OnDestinationReached(Walker arg1, GridNode arg2)
    {
        UpdateViewers();
        UpdateVisibility(); // TODO: is this necessary?
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
        List<GridEntity> enemies = NetworkMatchManager.Instance.GetEnemiesAs<GridEntity>(_unit);
        SeenByList.Clear();
        foreach (var enemy in enemies)
        {
            Viewer enemyViewer = enemy.GetComponent<Viewer>();
            if (!enemyViewer.Health.IsDead)
            {
                if (GridCoverManager.Instance.LineOfSight(enemy, _gridEntity, out Ray ray, out float rayLength, new List<GridNode[]>()))
                {
                    SeenByList.Add(enemyViewer);
                }
                if (GridCoverManager.Instance.LineOfSight(_gridEntity, enemy, out ray, out rayLength, new List<GridNode[]>()))
                {
                    SeeList.Add(enemyViewer);
                }
            }
        }
    }

    void UpdateVisibility()
    {
        Unit currentUnit = NetworkMatchManager.Instance.CurrentUnit;
        switch (ViewMode)
        {
            case ViewMode.Single:
                if (currentUnit != null)
                {
                    if (currentUnit.Team.Owner.isLocalPlayer)
                    {
                        IsVisible = _unit.Team.Owner.isLocalPlayer || SeenByList.Contains(currentUnit.GetComponent<Viewer>());
                    }
                    else
                    {
                        IsVisible = _unit.Team.Owner.isLocalPlayer || IsSeenByLocalPlayer();
                    }
                }
                else
                {
                    IsVisible = _unit.Team.Owner.isLocalPlayer || IsSeenByLocalPlayer();
                }
                break;
            case ViewMode.Team:
                IsVisible = (SeenByList.Count > 0) || (_unit.Team.Owner.isLocalPlayer && !_unit.Team.IsAI);
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
            if (viewer.Unit.Team.Owner.isLocalPlayer)
            {
                return true;
            }
        }
        return false;
    }
}
