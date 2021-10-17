using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;

public class GridNodeSelector : MonoBehaviour
{
    #region Fields

    [SerializeField] GameObject _areaPrefab;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _markerPrefab;

    GridManager _gridManager;

    Thrower _thrower;
    GridEntity _gridEntity;
    GameObject _area;
    GameObject _marker;

    public bool IsActive { get; private set; }

    GridNode _cachedNode;
    GridNode _targetNode;

    #endregion

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
        _thrower = GetComponent<Thrower>();
        _marker = Instantiate(_markerPrefab, Vector3.zero, Quaternion.identity);
        _marker.SetActive(false);
        _marker.transform.Find("Line").GetComponent<LineRenderer>().material.color = Color.red;
        _marker.transform.SetParent(GameObject.Find("Scene UI").transform);
        _gridManager = GridManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive)
        {
            if (EventSystem.current.IsPointerOverGameObject() && EventSystem.current.gameObject.GetComponent<StandAloneInputModuleV2>().GetCurrentFocusedGameObjectPublic() != null)
            {
                HideArea();
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
                    HideArea();
                    ShowMarker(gridNode);
                    Vector3 impulse;
                    Vector3[] trajectory;
                    Vector3 origin;
                    List<Vector3> origins = new List<Vector3>() { _gridEntity.CurrentNode.FloorPosition + 2 * Vector3.up };
                    foreach (var sidestep in GridCoverManager.Instance.SideSteps(_gridEntity, gridNode))
                    {
                        origins.Add(sidestep.FloorPosition + 2 * Vector3.up);
                    }
                    if (GridNode.EuclideanDistance(_gridEntity.CurrentNode, gridNode) < _thrower.ThrowRange &&
                        TrajectoryPredictor.Instance.TrajectoryToTarget(origins, gridNode.FloorPosition, out impulse, out trajectory, out origin))
                    {
                        _targetNode = gridNode;
                        TrajectoryPredictor.Instance.ShowTrajectory(trajectory);
                        ShowArea(_targetNode, _thrower.Grenade.Radius);
                    }
                    else
                    {
                        _targetNode = null;
                        ShowNoAccessMarker(gridNode);
                    }
                }
                if (_targetNode != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _thrower.SetTarget(_targetNode);
                        Deactivate();
                    }
                }
            }
            else
            {
                HideArea();
                HideMarker();
                _cachedNode = null;
            }
            UpdateHighlight();
        }
    }

    void UpdateHighlight()
    {
        if (_area)
        {
            Collider[] colliders = Physics.OverlapSphere(_area.transform.position, 0.5f * _area.transform.localScale.x);
            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    Highlight obj = collider.transform.root.GetComponent<Highlight>();
                    if (obj != null) // only consider Highlights
                    {
                        Health health = obj.transform.root.GetComponent<Health>();
                        if (health != null && !health.IsDead)
                        {
                            obj.Highlighted = true;
                        }
                    }
                }
            }
            foreach (var gridNode in GridManager.Instance.GetGrid())
            {
                if (gridNode.HasFloor && (gridNode.FloorPosition - _area.transform.position).magnitude <= 0.5f * _area.transform.localScale.x)
                {
                    GridHighlightManager.Instance.HighlightNode(gridNode);
                }
            }
        }
    }

    void ShowArea(GridNode target, int range)
    {
        _area = Instantiate(_areaPrefab, Vector3.zero, Quaternion.identity);
        _area.transform.position = target.FloorPosition;
        _area.transform.localScale = 2 * Vector3.one * _thrower.Grenade.Radius * GridManager.Instance.XZScale;
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

    void HideArea()
    {
        Destroy(_area);
        TrajectoryPredictor.Instance.ClearTrajectory();
        _area = null;
    }

    void HideMarker()
    {
        _marker.SetActive(false);
    }

    public void Activate()
    {
        IsActive = true;
    }

    void Deactivate()
    {
        IsActive = false;
        HideArea();
        HideMarker();
    }

    public void Cancel()
    {
        Deactivate();
    }
}
