using UnityEngine;
using System.Collections;

public class GridHighlightManager : MonoBehaviour
{
    [SerializeField] GameObject _gridNodeHighlightPrefab;
    GridNodeHighlight[,,] _gridNodeHighlights;

    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    void Start()
    {
        Grid grid = GridManager.Instance.GetGrid();
        _gridNodeHighlights = new GridNodeHighlight[grid.GetLength(0), grid.GetLength(1), grid.GetLength(2)];

        foreach (var gridNode in grid.Nodes())
        {
            // Highlight
            if (gridNode.HasFloor)
            {
                GameObject obj = Instantiate(_gridNodeHighlightPrefab, gridNode.FloorPosition, Quaternion.identity);
                obj.transform.SetParent(GameObject.Find("GridNodeHighlights").transform);
                _gridNodeHighlights[gridNode.X, gridNode.Y, gridNode.Z] = obj.GetComponent<GridNodeHighlight>();
            }
        }
    }

    public void HighlightNode(GridNode node)
    {
        _gridNodeHighlights[node.X, node.Y, node.Z].Highlighted = true;
    }

    #region Singleton

    private static GridHighlightManager _instance;

    public static GridHighlightManager Instance { get { return _instance; } }

    #endregion
}
