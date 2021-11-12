using UnityEngine;
using UnityEditor;
using Mirror;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GridNeighborSelector : NetworkBehaviour
{
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _markerPrefab;

    GridManager _gridManager;
    Pathfinder _pathfinder;

    Healer _healer;
    GameObject _marker;
    GridEntity _gridEntity;
    GridAgent _gridAgent;

    GridNode _cachedNode;
    GridNode _targetNode;

    List<GridNode> _targetNodes = new List<GridNode>();

    public bool IsActive { get; private set; }

    private void Awake()
    {
        _healer = GetComponent<Healer>();
        _marker = Instantiate(_markerPrefab, Vector3.zero, Quaternion.identity);
        _marker.SetActive(false);
        _marker.transform.Find("Line").GetComponent<LineRenderer>().material.color = Color.green;
        _marker.transform.SetParent(GameObject.Find("Scene UI").transform);
        _gridManager = GridManager.Instance;
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
    }

    private void Start()
    {
        _pathfinder = Pathfinder.Instance;
    }

    private void Update()
    {
        if (IsActive)
        {
            if (EventSystem.current.IsPointerOverGameObject() && EventSystem.current.gameObject.GetComponent<StandAloneInputModuleV2>().GetCurrentFocusedGameObjectPublic() != null)
            {
                HideMarker();
                return;
            }

            GridNode gridNode = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] raycastHit = Physics.RaycastAll(ray, Mathf.Infinity, _layerMask);
            if (raycastHit.Length > 0)
            {
                int maxY = int.MinValue;
                float maxy = float.MinValue;

                for (int i = 0; i < raycastHit.Length; i++)
                {
                    GridNode node = _gridManager.GetGridNodeFromWorldPosition(raycastHit[i].point);
                    if (node == null) { continue; };
                    if (!node.IsWalkable) { continue; };
                    if (node.Y <= CameraController.Instance.Level && node.Y >= maxY && raycastHit[i].point.y > maxy)
                    {
                        maxY = node.Y;
                        maxy = raycastHit[i].point.y;
                        gridNode = node;
                    }
                }
            }

            if (gridNode != null)
            {
                if (gridNode != _cachedNode)
                {
                    _cachedNode = gridNode;
                    if (_targetNodes.Contains(gridNode))
                    {
                        Health occupier;
                        if (gridNode.GetOccupierOfType<Health>(out occupier))
                        {
                            if (occupier.IsFull())
                            {
                                ShowNoAccessMarker(gridNode);
                                _targetNode = null;
                            }
                            else
                            {
                                ShowMarker(gridNode);
                                _targetNode = gridNode;
                            }
                        }
                        else
                        {
                            ShowNoAccessMarker(gridNode);
                            _targetNode = null;
                        }                        
                    }
                    else
                    {
                        ShowNoAccessMarker(gridNode);
                        _targetNode = null;
                    }
                }
                if (_targetNode != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        SetTarget(_targetNode);
                        Deactivate();
                    }
                }
            }
            else
            {
                HideMarker();
                _cachedNode = null;
            }
            UpdateHighlight();
        }
    }

    void ShowMarker(GridNode target)
    {
        _marker.transform.position = target.FloorPosition + Vector3.up * 0.2f;
        _marker.transform.Find("Line").gameObject.SetActive(true);
        _marker.transform.Find("No Access").gameObject.SetActive(false);
        _marker.SetActive(true);
    }

    void ShowNoAccessMarker(GridNode target)
    {
        _marker.transform.position = target.FloorPosition + Vector3.up * 0.2f;
        _marker.transform.Find("Line").gameObject.SetActive(false);
        _marker.transform.Find("No Access").gameObject.SetActive(true);
        _marker.SetActive(true);
    }

    void HideMarker()
    {
        _marker.SetActive(false);
    }

    void UpdateHighlight()
    {
        if (_targetNode != null)
        {
            Highlight occupier;
            _targetNode.GetOccupierOfType<Highlight>(out occupier);
            occupier.Highlighted = true;
            GridHighlightManager.Instance.HighlightNode(_targetNode);
        }
    }

    void SetTarget(GridNode target)
    {
        CmdSetTarget(new Vector3Int(target.X, target.Y, target.Z));
    }

    [Command]
    void CmdSetTarget(Vector3Int target)
    {
        RpcSetTarget(target);
    }

    [ClientRpc]
    void RpcSetTarget(Vector3Int target)
    {
        GridNode targetNode = _gridManager.GetGridNode(target.x, target.y, target.z);
        GridEntity targetEntity;
        targetNode.GetOccupierOfType<GridEntity>(out targetEntity);
        _healer.SetTarget(targetEntity);
    }


    public void Activate()
    {
        IsActive = true;
        GridNode origin = _gridEntity.CurrentNode;
        float maxJumpUp = _gridAgent.MaxJumpUp;
        float maxJumpDown = _gridAgent.MaxJumpDown;
        _pathfinder.Initialize(_gridManager.GetGrid(), origin, origin, 1, maxJumpUp, maxJumpDown, (node) => { return true; }, false);
        _targetNodes = _pathfinder.GetNodes(1);
    }

    void Deactivate()
    {
        IsActive = false;
        HideMarker();
    }

    public void Cancel()
    {
        Deactivate();
    }
}