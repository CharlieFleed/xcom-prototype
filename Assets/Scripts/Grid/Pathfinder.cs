using System;
using System.Collections.Generic;
using UnityEngine;

public delegate bool IsNodeAvailableDelegate(GridNode n);

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private bool _allowDiagonals = false;

    private Grid _grid;
    int _gridSizeY;
    private GridNode _origin;
    private GridNode _destination;
    private IsNodeAvailableDelegate _IsNodeAvailable = delegate { return true; };
    bool _useLadders;

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
        foreach (GridNode node in _grid.Nodes())
        {
            node.Reset();
        }
        // Initialize the origin node
        _origin.Distance = 0;
        _origin.Closed = true;
        //
        GridNode current = _origin;
        List<GridNode> neighbors = new List<GridNode>();
        HashSet<GridNode> openList = new HashSet<GridNode>();
        openList.Add(current);
        bool moreNodes = true;
        while (moreNodes)
        {
            neighbors = GetNeighbors(current, maxJumpUp, maxJumpDown);
            foreach (GridNode n in neighbors)
            {
                // if not visited
                if (!n.Closed)
                {
                    // add to the open list
                    if (!openList.Contains(n))
                        openList.Add(n);
                    // Update the distance from origin
                    if (current.Distance + 1 < n.Distance)
                    {
                        n.Distance = current.Distance + 1;
                        n.Parent = current;
                    }
                }
            }
            // mark current node as visited
            current.Closed = true;
            openList.Remove(current);
            // select the next node to be visited: it is the unvisited node in the open list with minimum distance
            moreNodes = false;
            int minDistance = int.MaxValue;
            // look for next node
            GridNode nextNode = current;
            foreach (var node in openList)
            {
                if (node.Distance <= minDistance && node.Distance < maxDistance)
                {
                    nextNode = node;
                    minDistance = nextNode.Distance;
                    moreNodes = true;
                }
            }
            if (moreNodes)
            {
                current = nextNode;
            }
        }
        // all eligible nodes have been visited
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
                if (yOffset > 0 && !_grid.Node(node.X, node.Y + 1, node.Z).IsAir)
                {
                    break;
                }
                GridNode neighbor = _grid.Node(node.X + xzOffsets[i][0], node.Y + yOffset, node.Z + xzOffsets[i][1]);
                // check if the candidate point belongs to the map
                if (neighbor != null)
                {
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
                        GridNode nodeAboveNeighbor = _grid.Node(node.X + xzOffsets[i][0], node.Y + 1, node.Z + xzOffsets[i][1]);
                        if (nodeAboveNeighbor.HasFloor && nodeAboveNeighbor.FloorPosition.y - node.FloorPosition.y < GridManager.Instance.UnitColliderExtents.y)
                        {
                            continue;
                        }
                    }
                    // we passed all the tests, select the neighbor and stop for this direction
                    neighbors.Add(_grid.Node(node.X + xzOffsets[i][0], node.Y + yOffset, node.Z + xzOffsets[i][1]));
                    break;
                }
            }
        }
        return neighbors;
    }

    public void Initialize(Grid grid, GridNode origin, GridNode destination, int maxDistance, float maxJumpUp, float maxJumpDown, IsNodeAvailableDelegate IsNodeAvailable, bool useLadders)
    {
        _grid = grid;
        _origin = origin;
        _destination = destination;
        _IsNodeAvailable = IsNodeAvailable;
        _useLadders = useLadders;
        //
        _gridSizeY = _grid.GetLength(1);
        UpdateDistances(maxDistance, maxJumpUp, maxJumpDown);
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
        if (_allowDiagonals)
        {
            path = DiagonalizePath(path);
        }
        return path;
    }

    Stack<GridNode> DiagonalizePath(Stack<GridNode> path)
    {
        if (path.Count <= 2)
        {
            return path;
        }
        Stack<GridNode> diagPath = new Stack<GridNode>();
        GridNode[] pathArray = new GridNode[path.Count];
        for (int i = 0; i < pathArray.Length; i++)
        {
            pathArray[pathArray.Length - i - 1] = path.Pop();
        }
        for (int i = 0; i < pathArray.Length - 2; i++)
        {
            GridNode d1 = pathArray[i];
            GridNode d2 = pathArray[i + 2];
            // if they are diagonal neighbors at the same height
            if (GridNode.AreDiagonals(d1, d2))
            {
                GridNode n1 = _grid.Node(d1.X, d1.Y, d2.Z);
                GridNode n2 = _grid.Node(d2.X, d2.Y, d1.Z);
                // if both intermediate nodes have d1 and d2 as neighbours and have the same height as n1/n2
                if (GetNeighbors(n1, 0, 0).Contains(d1) && GetNeighbors(n1, 0, 0).Contains(d2) && GetNeighbors(n2, 0, 0).Contains(d1) && GetNeighbors(n2, 0, 0).Contains(d2))
                {
                    if (!(GridNode.HalfWallBetween(n1, d1) || GridNode.HalfWallBetween(n1, d2) || GridNode.HalfWallBetween(n2, d1) || GridNode.HalfWallBetween(n2, d2)))
                    {
                        // skip intermediate node
                        diagPath.Push(d1);
                        if (i == pathArray.Length - 3)
                        {
                            diagPath.Push(d2);
                            return diagPath;
                        }
                        i++;
                        continue;
                    }
                }
            }
            // if we got here just add d1 to the path
            diagPath.Push(d1);
        }
        // if we got here add the last two nodes to the path
        diagPath.Push(pathArray[pathArray.Length - 2]);
        diagPath.Push(pathArray[pathArray.Length - 1]);
        return diagPath;
    }

    public List<GridNode> GetNodesWithinDistance(int distance)
    {
        List<GridNode> nodes = new List<GridNode>();
        foreach (GridNode node in _grid.Nodes())
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
