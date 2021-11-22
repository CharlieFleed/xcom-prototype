using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BorderVertices
{
    public GridNode gridNode;
    public bool NW, NE, SW, SE;
    public bool Visited { get { return !NW && !NE && !SW && !SE; } }
}

public enum VertexPosition { NW, NE, SW, SE }

public class GridRegionHighlighter : MonoBehaviour
{
    Dictionary<GridNode, BorderVertices> _border = new Dictionary<GridNode, BorderVertices>();
    GridManager _gm;
    List<Vector3> _vertices = new List<Vector3>();
    List<LineRenderer>[] _lrs = new List<LineRenderer>[] { new List<LineRenderer>(), new List<LineRenderer>() };

    List<LineRenderer> _lrPool = new List<LineRenderer>();
    List<BorderVertices> _bvPool = new List<BorderVertices>();

    Color _color1 = Color.cyan;
    Color _color2 = new Color(255 / 255f, 216 / 255f, 122 / 255f);

    [SerializeField] GameObject _lineRendererPrefab;
    [SerializeField] Material _transparentMaterial;
    [SerializeField] Material _fullMaterial;

    Vector3 _tileExtents;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _gm = GridManager.Instance;
        _tileExtents = _gm.TileExtents;
    }

    public void HighlightRegion(List<GridNode> region, int index)
    {
        UnityEngine.Profiling.Profiler.BeginSample("My Sample1");
        //Debug.Log(region.Count);
        Clear(index);
        _border.Clear();
        ClearBorder();
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("My Sample2");
        foreach (var node in region)
        {
            node.InRegion = true;
        }
        foreach (var node in region)
        {
            BorderVertices borderNode = NewBorderVertices();
            if (!((North(node) != null && North(node).InRegion) && (NorthEast(node) != null && NorthEast(node).InRegion) && (East(node) != null && East(node).InRegion)))
            {
                borderNode.NE = true;
            }
            if (!((North(node) != null && North(node).InRegion) && (NorthWest(node) != null && NorthWest(node).InRegion) && (West(node) != null && West(node).InRegion)))
            {
                borderNode.NW = true;
            }
            if (!((South(node) != null && South(node).InRegion) && (SouthEast(node) != null && SouthEast(node).InRegion) && (East(node) != null && East(node).InRegion)))
            {
                borderNode.SE = true;
            }
            if (!((South(node) != null && South(node).InRegion) && (SouthWest(node) != null && SouthWest(node).InRegion) && (West(node) != null && West(node).InRegion)))
            {
                borderNode.SW = true;
            }
            if (borderNode.NE || borderNode.NW || borderNode.SE || borderNode.SW)
            {
                borderNode.gridNode = node;
                _border.Add(node, borderNode);
            }
        }
        foreach (var node in region)
        {
            node.InRegion = false;
        }
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("My Sample3");
        while (_border.Keys.Count > 0)
        {
            _vertices.Clear();
            BorderVertices start = _border[_border.Keys.First()];
            VertexPosition startVertex = start.NE ? VertexPosition.NE : (start.NW ? VertexPosition.NW : (start.SE ? VertexPosition.SE : VertexPosition.SW));
            // create the line renderer
            LineRenderer lr = NewLineRenderer();
            if (index == 0 && _lrs[1].Count > 0)
            {
                lr.material = _transparentMaterial;
            }
            else
            {
                lr.material = _fullMaterial;
            }
            lr.material.color = (index == 0) ? _color1 : _color2;
            lr.sortingOrder = index;
            _lrs[index].Add(lr);
            //
            BorderVertices current = start;
            VertexPosition currentVertex = startVertex;
            AddVertex(current.gridNode, currentVertex, lr);
            //Debug.Log($"current node:{current.gridNode.X},{current.gridNode.Y},{current.gridNode.Z}; vertex:{currentVertex}.");
            NextVertex(ref current, ref currentVertex);
            //Debug.Log($"current node:{current.gridNode.X},{current.gridNode.Y},{current.gridNode.Z}; vertex:{currentVertex}.");
            while (!(current == start && currentVertex == startVertex))
            {
                AddVertex(current.gridNode, currentVertex, lr);
                NextVertex(ref current, ref currentVertex);
                //Debug.Log($"HA!HA! current node:{current.gridNode.X},{current.gridNode.Y},{current.gridNode.Z}; vertex:{currentVertex}.");
            }
            AddIntermediatePoints(_vertices.Last(), _vertices.First());
            lr.positionCount = _vertices.Count;
            lr.SetPositions(_vertices.ToArray());
            PurgeBorder(index);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void PurgeBorder(int index)
    {
        GridNode[] keys = _border.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++)
        {
            if (_border[keys[i]].Visited)
            {
                _border.Remove(keys[i]);
            }
        }
    }

    public void Clear(int index)
    {
        foreach (var lr in _lrs[index])
        {
            RecycleLineRenderer(lr);
        }
        _lrs[index].Clear();
    }

    void ClearBorder()
    {
        foreach (var bv in _border)
        {
            RecycleBorderVertices(bv.Value);
        }
        _border.Clear();
    }

    void AddVertex(GridNode n, VertexPosition vertexPosition, LineRenderer lr)
    {
        Vector3 newVertex = Vector3.zero;
        switch (vertexPosition)
        {
            case VertexPosition.NW:
                newVertex = n.FloorPosition + new Vector3(-0.5f * _tileExtents.x, 0.2f, 0.5f * _tileExtents.z);
                _border[n].NW = false;
                break;
            case VertexPosition.NE:
                newVertex = n.FloorPosition + new Vector3(0.5f * _tileExtents.x, 0.2f, 0.5f * _tileExtents.z);
                _border[n].NE = false;
                break;
            case VertexPosition.SW:
                newVertex = n.FloorPosition + new Vector3(-0.5f * _tileExtents.x, 0.2f, -0.5f * _tileExtents.z);
                _border[n].SW = false;
                break;
            case VertexPosition.SE:
                newVertex = n.FloorPosition + new Vector3(0.5f * _tileExtents.x, 0.2f, -0.5f * _tileExtents.z);
                _border[n].SE = false;
                break;
        }
        if (_vertices.Count > 0)
        {
            Vector3 prevVertex = _vertices.Last();
            AddIntermediatePoints(prevVertex, newVertex);
        }
        _vertices.Add(newVertex);      
        //Debug.Log($"Added position: {lr.GetPosition(lr.positionCount - 1).x},{lr.GetPosition(lr.positionCount - 1).y},{lr.GetPosition(lr.positionCount - 1).z}");
    }

    void AddIntermediatePoints(Vector3 prevVertex, Vector3 newVertex)
    {
        // if we are going up, get to 40% on the XZ plane, move up and finally to destination
        if (newVertex.y > prevVertex.y)
        {
            Vector3 position = Vector3.Lerp(prevVertex, newVertex, 0.4f);
            position.y = prevVertex.y;
            _vertices.Add(position);
            position.y = newVertex.y;
            _vertices.Add(position);
        }
        // if we are going down, get to 60% on the XZ plane, move down and finally to destination
        if (newVertex.y < prevVertex.y)
        {
            Vector3 position = Vector3.Lerp(prevVertex, newVertex, 0.6f);
            position.y = prevVertex.y;
            _vertices.Add(position);
            position.y = newVertex.y;
            _vertices.Add(position);
        }
    }

    void NextVertex(ref BorderVertices n, ref VertexPosition vertex)
    {
        switch (vertex)
        {
            case VertexPosition.NW:
                if (North(n.gridNode) != null && _border.Keys.Contains(North(n.gridNode)))
                {
                    n = _border[North(n.gridNode)];
                    vertex = VertexPosition.SW;
                }
                else
                {
                    vertex = VertexPosition.NE;
                }
                break;
            case VertexPosition.NE:
                if (East(n.gridNode) != null && _border.Keys.Contains(East(n.gridNode)))
                {
                    n = _border[East(n.gridNode)];
                    vertex = VertexPosition.NW;
                }
                else
                {
                    vertex = VertexPosition.SE;
                }
                break;
            case VertexPosition.SW:
                if (West(n.gridNode) != null && _border.Keys.Contains(West(n.gridNode)))
                {
                    n = _border[West(n.gridNode)];
                    vertex = VertexPosition.SE;
                }
                else
                {
                    vertex = VertexPosition.NW;
                }
                break;
            case VertexPosition.SE:
                if (South(n.gridNode) != null && _border.Keys.Contains(South(n.gridNode)))
                {
                    n = _border[South(n.gridNode)];
                    vertex = VertexPosition.NE;
                }
                else
                {
                    vertex = VertexPosition.SW;
                }
                break;
        }
    }

    GridNode North(GridNode n)
    {
        return _gm.GetGridNode(n.X, n.Y, n.Z + 1);
    }
    GridNode South(GridNode n)
    {
        return _gm.GetGridNode(n.X, n.Y, n.Z - 1);
    }
    GridNode East(GridNode n)
    {
        return _gm.GetGridNode(n.X + 1, n.Y, n.Z);
    }
    GridNode West(GridNode n)
    {
        return _gm.GetGridNode(n.X - 1, n.Y, n.Z);
    }
    GridNode NorthEast(GridNode n)
    {
        return _gm.GetGridNode(n.X + 1, n.Y, n.Z + 1);
    }
    GridNode NorthWest(GridNode n)
    {
        return _gm.GetGridNode(n.X - 1, n.Y, n.Z + 1);
    }
    GridNode SouthEast(GridNode n)
    {
        return _gm.GetGridNode(n.X + 1, n.Y, n.Z - 1);
    }
    GridNode SouthWest(GridNode n)
    {
        return _gm.GetGridNode(n.X - 1, n.Y, n.Z - 1);
    }

    public Vector2 offsetSpeed = Vector2.right;

    void Update()
    {
        if (_lrs[1].Count > 0)
        {
            foreach (LineRenderer lr in _lrs[0])
            {
                lr.material.mainTextureOffset = offsetSpeed * Time.realtimeSinceStartup;
            }
        }
    }

    LineRenderer NewLineRenderer()
    {
        LineRenderer lr;
        if (_lrPool.Count > 0)
        {
            lr = _lrPool[0];
            _lrPool.RemoveAt(0);
        }
        else
        {
            //Debug.Log("New Line Renderer");
            GameObject obj = Instantiate(_lineRendererPrefab, transform);
            lr = obj.GetComponent<LineRenderer>();
        }
        return lr;
    }
    void RecycleLineRenderer(LineRenderer lr)
    {
        _lrPool.Add(lr);
        lr.positionCount = 0;
    }

    BorderVertices NewBorderVertices()
    {
        BorderVertices bv;
        if (_bvPool.Count > 0)
        {
            bv = _bvPool[0];
            _bvPool.RemoveAt(0);
        }
        else
        {
            bv = new BorderVertices();
        }
        return bv;
    }
    void RecycleBorderVertices(BorderVertices bv)
    {
        _bvPool.Add(bv);
    }

    #region Singleton

    private static GridRegionHighlighter _instance;

    public static GridRegionHighlighter Instance { get { return _instance; } }

    #endregion
}
