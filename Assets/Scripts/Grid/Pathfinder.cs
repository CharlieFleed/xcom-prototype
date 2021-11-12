using System;
using System.Collections.Generic;
using UnityEngine;

public delegate bool IsNodeAvailableDelegate(GridNode n);

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private bool allowDiagonals = false;

    private GridNode[,,] _gridNodes;
    int _gridSizeX;
    int _gridSizeY;
    int _gridSizeZ;
    private GridNode _origin;
    private GridNode _destination;
    private IsNodeAvailableDelegate _IsNodeAvailable = delegate { return true; };
    bool _useLadders;

    private Stack<GridNode> _path = new Stack<GridNode>();

    private void Awake()
    {
        _instance = this;    
    }

    /// <summary>
    /// Calculates distances of all nodes from current "origin". Stops when it reaches all nodes at distance = maxDistance.
    /// </summary>
    private void UpdateDistances(int maxDistance, float maxJumpUp, float maxJumpDown)
    {
        // Initialize nodes
        foreach (GridNode node in _gridNodes)
        {
            node.Reset();
        }
        // Initialize the origin node
        _origin.Distance = 0;
        _origin.Visited = true;
        //
        GridNode current = _origin;
        List<GridNode> neighbors = new List<GridNode>();
        bool moreNodes = true;
        while (moreNodes)
        {
            neighbors = GetNeighbors(current, maxJumpUp, maxJumpDown);
            foreach (GridNode n in neighbors)
            {
                // if reachable and not visited
                if (n.IsWalkable && !n.Visited)
                {
                    // Update the distance from origin
                    if (current.Distance + 1 < n.Distance)
                    {
                        n.Distance = current.Distance + 1;
                        n.Parent = current;
                    }
                }
            }
            // mark current node as visited
            current.Visited = true;
            // select the next node to be visited: it is the unvisited node with minimum distance
            moreNodes = false;
            int minDistance = int.MaxValue;
            // look for next node
            GridNode nextNode = current;
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    for (int z = 0; z < _gridSizeZ; z++)
                    {
                        // if reachable and not visited
                        if (_gridNodes[x, y, z].IsWalkable && !_gridNodes[x, y, z].Visited)
                        {
                            if (_gridNodes[x, y, z].Distance <= minDistance && _gridNodes[x, y, z].Distance < maxDistance)
                            {
                                nextNode = _gridNodes[x, y, z];
                                minDistance = nextNode.Distance;
                                moreNodes = true;
                            }
                        }
                    }
                }
            }
            if (moreNodes)
            {
                current = nextNode;
            }
        }
        // all nodes have been visited
        return;
    }

    private List<GridNode> GetNeighbors(GridNode node, float maxJumpUp, float maxJumpDown)
    {
        List<GridNode> neighbors = new List<GridNode>();
        if (_useLadders)
        {
            foreach (var ladderNeighbor in node.LadderNeighbors)
            {
                if (_IsNodeAvailable(ladderNeighbor))
                {
                    neighbors.Add(ladderNeighbor);
                }
            }
        }
        int[][] xzOffsets = { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
        int[] yOffsets = new int[_gridSizeY];
        yOffsets[0] = 0;
        for (int y = 1; y <= node.Y; y++)
        {
            yOffsets[y] = -y;
        }
        for (int y = node.Y + 1; y < _gridSizeY; y++)
        {
            yOffsets[y] = y - node.Y;
        }
        for (int i = 0; i < xzOffsets.Length; i++)
        {
            bool canJumpDown = true;
            foreach (int yOffset in yOffsets)
            {
                // stop for this direction if there is a wall
                if (yOffset == 0 && node.Walls[i])
                {
                    break;
                }
                // skip lower floors if cannot jump down
                if (yOffset < 0 && !canJumpDown)
                {
                    continue;
                }
                // skip upper floors and stop for this direction if node above is not air
                if (yOffset > 0 && !_gridNodes[node.X, node.Y + 1, node.Z].IsAir)
                {
                    break;
                }
                // check if the candidate point belongs to the map
                if (node.X + xzOffsets[i][0] > -1 && node.X + xzOffsets[i][0] < _gridSizeX &&
                    node.Y + yOffset > -1 && node.Y + yOffset < _gridSizeY &&
                    node.Z + xzOffsets[i][1] > -1 && node.Z + xzOffsets[i][1] < _gridSizeZ)
                {
                    GridNode neighbor = _gridNodes[node.X + xzOffsets[i][0], node.Y + yOffset, node.Z + xzOffsets[i][1]];
                    // check if walkable
                    if (!neighbor.IsWalkable)
                    {
                        if (yOffset == 0 && !neighbor.IsAir)
                        {
                            canJumpDown = false;
                        }
                        continue;
                    }
                    // There is a walkable tile at this level
                    canJumpDown = false;
                    // check height difference
                    if (node.FloorPosition.y - neighbor.FloorPosition.y > maxJumpDown || neighbor.FloorPosition.y - node.FloorPosition.y > maxJumpUp)
                    {
                        continue;
                    }
                    // check if available
                    if (!_IsNodeAvailable(neighbor))
                    {
                        continue;
                    }
                    // skip if node above neighbor doesn't leave room
                    if (yOffset == 0 && (node.Y + 1) < _gridSizeY)
                    {
                        GridNode nodeAboveNeighbor = _gridNodes[node.X + xzOffsets[i][0], node.Y + 1, node.Z + xzOffsets[i][1]];
                        if (nodeAboveNeighbor.HasFloor && nodeAboveNeighbor.FloorPosition.y - node.FloorPosition.y < GridManager.Instance.UnitColliderExtents.y)
                        {
                            continue;
                        }
                    }
                    // we passed all the tests, select the neighbor and stop
                    neighbors.Add(_gridNodes[node.X + xzOffsets[i][0], node.Y + yOffset, node.Z + xzOffsets[i][1]]);
                    break;
                }
            }
        }
        return neighbors;
    }

    private List<GridNode> GetDiagNeighbors(GridNode node, float maxJumpUp, float maxJumpDown)
    {
        List<GridNode> neighbors = GetNeighbors(node, maxJumpUp, maxJumpDown);
        List<GridNode> diagNeighbors = new List<GridNode>();
        int[][] diagOffsets = { new int[] { 1, 0, 1 }, new int[] { 1, 0, -1 }, new int[] { -1, 0, 1 }, new int[] { -1, 0, -1 } };
        foreach (int[] offset in diagOffsets)
        {
            // check if the candidate point belongs to the map
            if (node.X + offset[0] > -1 && node.X + offset[0] < _gridSizeX &&
                node.Y + offset[1] > -1 && node.Y + offset[1] < _gridSizeY && 
                node.Z + offset[2] > -1 && node.Z + offset[2] < _gridSizeZ)
            {
                GridNode n = _gridNodes[node.X + offset[0], node.Y, node.Z + offset[2]];
                GridNode n1 = _gridNodes[node.X + offset[0], node.Y, node.Z];
                GridNode n2 = _gridNodes[node.X, node.Y, node.Z + offset[2]];

                // check both n1 and n2 are mutual neighbors
                List<GridNode> neighborsOfN = GetNeighbors(n, maxJumpUp, maxJumpDown);
                if (!neighbors.Contains(n1) || !neighbors.Contains(n2) || !neighborsOfN.Contains(n1) || !neighborsOfN.Contains(n2))
                {
                    //Debug.Log($"DiagNeighbors for {node.X}, {node.Y}, {node.Z}. Neighbors check failed on candidate {node.X + offset[0]}, {node.Y}, {node.Z + offset[2]}");
                    continue;
                }
                // check height difference
                if (Mathf.Abs(node.FloorPosition.y - n.FloorPosition.y) > maxJumpUp || Mathf.Abs(node.FloorPosition.y - n1.FloorPosition.y) > maxJumpUp || Mathf.Abs(node.FloorPosition.y - n2.FloorPosition.y) > maxJumpUp)
                {
                    continue;
                }
                // check if available
                if (!_IsNodeAvailable(n) || !_IsNodeAvailable(n1) || !_IsNodeAvailable(n2))
                {
                    continue;
                }
                //Debug.Log($"DiagNeighbors for {node.X}, {node.Y}, {node.Z}. Adding {n.X}, {n.Y}, {n.Z}");
                diagNeighbors.Add(n);
            }
        }
        return diagNeighbors;
    }

    /// <summary>
    /// Obsolete
    /// </summary>
    /// <param name="maxJumpUp"></param>
    /// <param name="maxJumpDown"></param>
    private void UpdatePath(float maxJumpUp, float maxJumpDown)
    {
        _path.Clear();
        List<GridNode> neighbors = new List<GridNode>();
        GridNode current = _destination;
        GridNode next;
        _path.Push(current);
        while (current != _origin)
        {
            next = current;
            neighbors = GetNeighbors(current, maxJumpDown, maxJumpUp);// NOTE: path in reverse
            if (allowDiagonals)
                neighbors.AddRange(GetDiagNeighbors(current, maxJumpDown, maxJumpUp)); 
            float minDistance = float.PositiveInfinity;
            foreach (GridNode n in neighbors)
            {
                // check if the neighbor is closer to the "origin" and closer of the current next node
                if (n.Distance < current.Distance)
                {
                    if (GridNode.EuclideanDistance(n, _origin) <= minDistance)
                    { 
                        next = n;
                        minDistance = GridNode.EuclideanDistance(n, _origin);
                    }
                }
            }            
            if (next == current)
            {
                //Debug.Log("Target not reachable.");
                // no progress, the target cannot be reached
                return;
            }
            else
            {
                current = next;
                _path.Push(current);
            }
        }
        //Debug.Log($"Path:");
        //foreach (var node in _path)
        //{
        //    Debug.Log($"  {node.X}, {node.Y}, {node.Z}.");
        //}
        return;
    }

    private void UpdatePath()
    {
        _path.Clear();
        GridNode current = _destination;
        while (current.Parent != null)
        {
            _path.Push(current);
            current = current.Parent;
        }
        _path.Push(current);
    }

    public void Initialize(GridNode[,,] gridNodes, GridNode origin, GridNode destination, int maxDistance, float maxJumpUp, float maxJumpDown, IsNodeAvailableDelegate IsNodeAvailable, bool useLadders)
    {
        _gridNodes = gridNodes;
        _origin = origin;
        _destination = destination;
        _IsNodeAvailable = IsNodeAvailable;
        _useLadders = useLadders;
        _gridSizeX = gridNodes.GetLength(0);
        _gridSizeY = gridNodes.GetLength(1);
        _gridSizeZ = gridNodes.GetLength(2);
        UpdateDistances(maxDistance, maxJumpUp, maxJumpDown);
        UpdatePath();
    }

    public Stack<GridNode> GetPathTo(GridNode destinationNode)
    {
        Stack<GridNode> path = new Stack<GridNode>();
        GridNode current = destinationNode;
        while (current.Parent != null)
        {
            path.Push(current);
            current = current.Parent;
        }
        path.Push(current);
        return path;
    }

    public List<GridNode> GetNodes(int distance)
    {
        List<GridNode> nodes = new List<GridNode>();
        foreach (GridNode node in _gridNodes)
        {
            if (node.Distance <= distance)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    #region Singleton

    private static Pathfinder _instance;

    public static Pathfinder Instance { get { return _instance; } }

    #endregion
}
