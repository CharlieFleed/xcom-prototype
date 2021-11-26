using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class GridPathSelector : NetworkBehaviour
{
    #region Fields

    [SerializeField] GameObject _markerPrefab;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] CoverHighlights _coverHighlightsPrefab;
    [SerializeField] GameObject _pathLineRendererPrefab;

    public bool IsActive { get; private set; }

    Pathfinder _pathfinder;
    GridManager _gridManager;
    LineRenderer _lineRenderer;
    GameObject _marker;
    LineRenderer _markerLineRenderer;

    GridNode _origin;
    Stack<GridNode> _path = new Stack<GridNode>();
    Walker _walker;
    GridEntity _gridEntity;
    GridAgent _gridAgent;
    List<GridNode> _walkRegion = new List<GridNode>();
    List<GridNode> _runRegion = new List<GridNode>();
    CoverHighlights[] _coverHighlights = new CoverHighlights[9];

    GridRegionHighlighter _gridRegionHighlighter;

    GridNode _cachedNode;
    GridNode _targetNode;
    int _cost;

    Color _color1 = Color.cyan;
    Color _color2 = new Color(255 / 255f, 216 / 255f, 122 / 255f);

    GameObject _sceneUI;

    InputCache _input = new InputCache();

    #endregion

    private void Awake()
    {
        _walker = GetComponent<Walker>();
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
        GameObject obj = Instantiate(_pathLineRendererPrefab, transform);
        _lineRenderer = obj.GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _marker = Instantiate(_markerPrefab, Vector3.zero, Quaternion.identity);
        _marker.SetActive(false);
        _marker.transform.SetParent(GameObject.Find("Scene UI").transform);
        _markerLineRenderer = _marker.GetComponentInChildren<LineRenderer>();
        CameraController.Instance.OnLevelChanged += HandleCameraController_OnLevelChanged;
        _sceneUI = GameObject.Find("Scene UI");
        for (int i = 0; i < 9; i++)
        {
            CoverHighlights ch = Instantiate(_coverHighlightsPrefab, _sceneUI.transform);
            _coverHighlights[i] = ch;
        }
    }

    private void Start()
    {
        _pathfinder = Pathfinder.Instance;
        _gridManager = GridManager.Instance;
        _gridRegionHighlighter = GridRegionHighlighter.Instance;
    }

    private void HandleCameraController_OnLevelChanged()
    {
        if (IsActive)
        {
            int maxDistance = _gridAgent.WalkRange * _walker._NumMoves;
            UpdateRanges(maxDistance - _gridAgent.WalkRange, maxDistance);
            _cachedNode = null; // to force an update
        }
    }

    bool IsNodeAvailable(GridNode node)
    {
        return _gridManager.IsNodeAvailable(node, _gridAgent);
    }

    void ShowHighlights(Stack<GridNode> path, int moves, int cost)
    {
        ShowPath(path, moves, cost);
        ShowMarker(path, moves, cost);
        ShowCover(path);
        ShowRanges(moves, cost);
    }

    void ShowRanges(int moves, int cost)
    {
        if (moves == 2)
        {
            if (cost == 1)
            {
                _gridRegionHighlighter.HighlightRegion(_walkRegion, 0);
            }
            else
            {
                _gridRegionHighlighter.HighlightRegion(_runRegion, 1);
                _gridRegionHighlighter.HighlightRegion(_walkRegion, 0);
            }
        }
        else
        {
            _gridRegionHighlighter.HighlightRegion(_runRegion, 1);
        }
    }

    void ShowPath(Stack<GridNode> path, int moves, int cost)
    {
        _lineRenderer.positionCount = path.Count;
        _lineRenderer.startColor = (moves - cost > 0) ? _color1 : _color2;
        _lineRenderer.endColor = (moves - cost > 0) ? _color1 : _color2;
        int addedPositions = 0;
        GridNode[] pathArray = path.ToArray();
        for (int i = 0; i < pathArray.Length; i++)
        {
            GridNode n = pathArray[i];
            if (i > 0)
            {
                // if we are going up, get to 40% on the XZ plane, move up and finally to destination
                if (n.FloorPosition.y > pathArray[i - 1].FloorPosition.y)
                {
                    _lineRenderer.positionCount += 2;
                    Vector3 position = Vector3.Lerp(pathArray[i - 1].FloorPosition, n.FloorPosition, 0.4f);
                    position.y = pathArray[i - 1].FloorPosition.y + 0.2f;
                    _lineRenderer.SetPosition(i + addedPositions, position);
                    //Debug.Log($"Point {_lineRenderer.GetPosition(i + addedPositions).x}, {_lineRenderer.GetPosition(i + addedPositions).y}, {_lineRenderer.GetPosition(i + addedPositions).z}.");
                    addedPositions++;
                    position.y = n.FloorPosition.y + 0.2f;
                    _lineRenderer.SetPosition(i + addedPositions, position);
                    //Debug.Log($"Point {_lineRenderer.GetPosition(i + addedPositions).x}, {_lineRenderer.GetPosition(i + addedPositions).y}, {_lineRenderer.GetPosition(i + addedPositions).z}.");
                    addedPositions++;
                }
                // if we are going down, get to 60% on the XZ plane, move down and finally to destination
                if (n.FloorPosition.y < pathArray[i - 1].FloorPosition.y)
                {
                    _lineRenderer.positionCount += 2;
                    Vector3 position = Vector3.Lerp(pathArray[i - 1].FloorPosition, n.FloorPosition, 0.6f);
                    position.y = pathArray[i - 1].FloorPosition.y + 0.2f;
                    _lineRenderer.SetPosition(i + addedPositions, position);
                    //Debug.Log($"Point {_lineRenderer.GetPosition(i + addedPositions).x}, {_lineRenderer.GetPosition(i + addedPositions).y}, {_lineRenderer.GetPosition(i + addedPositions).z}.");
                    addedPositions++;
                    position.y = n.FloorPosition.y + 0.2f;
                    _lineRenderer.SetPosition(i + addedPositions, position);
                    //Debug.Log($"Point {_lineRenderer.GetPosition(i + addedPositions).x}, {_lineRenderer.GetPosition(i + addedPositions).y}, {_lineRenderer.GetPosition(i + addedPositions).z}.");
                    addedPositions++;
                }
            }
            _lineRenderer.SetPosition(i + addedPositions, n.FloorPosition + Vector3.up * 0.2f);
            //Debug.Log($"Point {_lineRenderer.GetPosition(i + addedPositions).x}, {_lineRenderer.GetPosition(i + addedPositions).y}, {_lineRenderer.GetPosition(i + addedPositions).z}.");
        }
    }

    void ShowMarker(Stack<GridNode> path, int moves, int cost)
    {
        GridNode n = path.ToArray()[path.Count - 1];
        _marker.transform.position = n.FloorPosition + Vector3.up * 0.2f;
        _markerLineRenderer.material.color = (moves - cost > 0) ? _color1 : _color2;
        _marker.SetActive(true);
    }

    void ShowCover(Stack<GridNode> path)
    {
        //Debug.Log("ShowCover");
        GridNode n = path.ToArray()[path.Count - 1];
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                GridNode node = _gridManager.GetGridNode(n.X + x, n.Y, n.Z + z);
                CoverHighlights ch = _coverHighlights[(x + 1) + 3 * (z + 1)];
                if (node != null)
                {
                    ch.transform.position = node.FloorPosition;
                    if (x == 0 && z == 0)
                    {
                        ch.Main = true;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        //Debug.Log($"Node: {node.X},{node.Y},{node.Z} Walkable:{node.IsWalkable} Wall[{i}]{node.Walls[i]} HalfWall[{i}]{node.HalfWalls[i]} ");
                        ch._flags[i] = node.IsWalkable && node.Walls[i];
                        ch._hFlags[i] = node.IsWalkable && node.HalfWalls[i];
                    }
                }
            }
        }
    }

    void HideHighlights()
    {
        _lineRenderer.positionCount = 0;
        _marker.SetActive(false);
        for (int i = 0; i < 9; i++)
        {
            _coverHighlights[i].Main = false;
            for (int j = 0; j < 4; j++)
            {
                _coverHighlights[i]._flags[j] = false;
                _coverHighlights[i]._hFlags[j] = false;
            }
        }
        _gridRegionHighlighter.Clear(0);
        _gridRegionHighlighter.Clear(1);
    }

    void UpdateRanges(int walkDistance, int runDistance)
    {
        _walkRegion.Clear();
        _runRegion.Clear();
        foreach (GridNode n in _gridManager.GetGrid().Nodes())
        {
            if (n.Y <= CameraController.Instance.Level)
            {
                if (n.Distance == 0)
                {
                    if (walkDistance > 0)
                    {
                        _walkRegion.Add(n);
                    }
                    _runRegion.Add(n);
                }
                else if (n.Distance <= walkDistance)
                {
                    //_gridNodeHighlights[n.X, n.Y, n.Z].IsWalkRange = true;
                    _walkRegion.Add(n);
                    _runRegion.Add(n);
                }
                else if (n.Distance <= runDistance)
                {
                    //_gridNodeHighlights[n.X, n.Y, n.Z].IsRunRange = true;
                    _runRegion.Add(n);
                }
            }
        }
    }

    private void Update()
    {
        if (IsActive)
        {
            _input.Update();
        }
    }

    private void LateUpdate()
    {
        if (IsActive)
        {
            if (EventSystem.current.IsPointerOverGameObject() && EventSystem.current.gameObject.GetComponent<StandAloneInputModuleV2>().GetCurrentFocusedGameObjectPublic() != null)
            {
                HideHighlights();
                _cachedNode = null;
                return;
            }
            GridNode gridNode = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] raycastHit = Physics.RaycastAll(ray, Mathf.Infinity, _layerMask);
            if (raycastHit.Length > 0)
            {
                int Y = int.MinValue;
                float y = float.MinValue;

                for (int i = 0; i < raycastHit.Length; i++)
                {
                    GridNode node = _gridManager.GetGridNodeFromWorldPosition(raycastHit[i].point);
                    if (node == null) { continue; };
                    if (!node.IsWalkable) { continue; };
                    if (node.Y <= CameraController.Instance.Level && node.Y >= Y && raycastHit[i].point.y > y)
                    {
                        Y = node.Y;
                        y = raycastHit[i].point.y;
                        gridNode = node;
                    }
                }
            }
            if (gridNode != null)
            {
                //Debug.Log($"GridNode {gridNode.X}, {gridNode.Y}, {gridNode.Z}.");
                //if (_cachedNode != null)
                //{
                //    Debug.Log($"CachedNode {_cachedNode.X}, {_cachedNode.Y}, {_cachedNode.Z}.");
                //}
                //else
                //{
                //    Debug.Log($"CachedNode null.");
                //}
                if (gridNode != _cachedNode)
                {
                    _cachedNode = gridNode;
                    if (gridNode.Distance < int.MaxValue)
                    {
                        _targetNode = gridNode;
                    }
                    else
                    {
                        //Debug.Log($"gridNode {gridNode.X}, {gridNode.Y}, {gridNode.Z} not reachable. Distance: {gridNode.Distance}.");
                        GridNode bestMatchNode = null;
                        float closestDistance = float.MaxValue;
                        foreach (var node in _gridManager.GetGrid().Nodes())
                        {
                            if (node.Distance < int.MaxValue && GridNode.EuclideanDistance(node, gridNode) < closestDistance)
                            {
                                bestMatchNode = node;
                                closestDistance = GridNode.EuclideanDistance(node, gridNode);
                            }
                        }
                        _targetNode = bestMatchNode;
                        //Debug.Log($"targetNode is {_targetNode.X}, {_targetNode.Y}, {_targetNode.Z}. Distance: {_targetNode.Distance}.");
                    }
                    _cost = _targetNode.Distance <= _gridAgent.WalkRange ? 1 : 2;
                    HideHighlights();
                    _path = _pathfinder.GetPathTo(_targetNode);
                    //Debug.Log($"Path to {targetNode.X}, {targetNode.Y}, {targetNode.Z}.");
                    ShowHighlights(_path, _walker._NumMoves, _cost);
                }
                if (Input.GetMouseButtonDown(1))
                {
                    //Debug.Log($"Selected targetNode is {_targetNode.X}, {_targetNode.Y}, {_targetNode.Z}. Distance: {_targetNode.Distance}.");
                    SetPath(_targetNode);
                    Deactivate();
                }
            }
            else
            {
                //Debug.Log($"GridNode null.");
                HideHighlights();
                _cachedNode = null;
            }
        }
        _input.Clear();
    }

    void SetPath(GridNode target)
    {
        CmdSetPath(new Vector3Int(target.X, target.Y, target.Z));
    }

    [Command]
    void CmdSetPath(Vector3Int target)
    {
        RpcSetPath(target);
    }

    [ClientRpc]
    void RpcSetPath(Vector3Int target)
    {
        GridNode targetNode = _gridManager.GetGridNode(target.x, target.y, target.z);
        _origin = _gridEntity.CurrentNode;
        int maxDistance = _gridAgent.WalkRange * _walker._NumMoves;
        float maxJumpUp = _gridAgent.MaxJumpUp;
        float maxJumpDown = _gridAgent.MaxJumpDown;
        _pathfinder.Initialize(_gridManager.GetGrid(), _origin, _origin, maxDistance, maxJumpUp, maxJumpDown, IsNodeAvailable, true);
        UpdateRanges(maxDistance - _gridAgent.WalkRange, maxDistance);
        _cost = targetNode.Distance <= _gridAgent.WalkRange ? 1 : 2;
        _path = _pathfinder.GetPathTo(targetNode);
        _gridAgent.BookedNode = targetNode;
        targetNode.IsBooked = true;
        _walker.SetPath(_path, _cost);
    }

    public void Activate()
    {
        IsActive = true;
        _origin = _gridEntity.CurrentNode;
        int maxDistance = _gridAgent.WalkRange * _walker._NumMoves;
        float maxJumpUp = _gridAgent.MaxJumpUp;
        float maxJumpDown = _gridAgent.MaxJumpDown;
        _pathfinder.Initialize(_gridManager.GetGrid(), _origin, _origin, maxDistance, maxJumpUp, maxJumpDown, IsNodeAvailable, true);
        //Debug.Log($"_pathfinder.GetPath, maxDistance: {maxDistance}.");
        UpdateRanges(maxDistance - _gridAgent.WalkRange, maxDistance);
    }

    void Deactivate()
    {
        IsActive = false;
        HideHighlights();
        _cachedNode = null;
        _targetNode = null;
    }

    public void Cancel()
    {
        Deactivate();
    }
}
