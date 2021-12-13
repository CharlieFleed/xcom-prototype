using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridNode
{
    #region Fields

    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Z { get; set; } = 0;

    public Vector3 WorldPosition { get; set; } = Vector3.zero;
    public Vector3 FloorPosition { get; set; } = Vector3.zero;

    public bool HasFloor { get; set; } = false;
    /// <summary>
    /// Grid nodes with destructibles on top are still walkable.
    /// </summary>
    public bool IsWalkable { get; set; } = false;
    public bool IsAir { get; set; } = false;

    public bool Closed { get; set; } = false;
    public int Distance { get; set; } = int.MaxValue;

    public bool IsBooked { get; set; }

    public Vector3 UnitColliderExtents { get; set; }

    /// <summary>
    /// Right, Forward, Left, Back / East, North, West, South
    /// </summary>
    public bool[] Walls = { false, false, false, false };
    public bool[] HalfWalls = { false, false, false, false };
    public bool[] Ladders = { false, false, false, false };
    public enum Orientations { East, North, West, South};

    public List<GridNode> LadderNeighbors = new List<GridNode>();
    public GridNode Parent;

    /// <summary>
    /// Used by GridRegionHighlighter
    /// </summary>
    public bool InRegion;

    #endregion

    public void Reset()
    {
        Distance = int.MaxValue;
        Closed = false;
        Parent = null;
    }

    public bool IsOccupied()
    {
        int layerMask = ~LayerMask.GetMask("Cover");
        Collider[] colliders = Physics.OverlapBox(FloorPosition + 0.5f * Vector3.up * UnitColliderExtents.y, 0.5f * (UnitColliderExtents - Vector3.one * 0.001f), Quaternion.identity, layerMask); // NOTE: shrunk to avoid collision at the edges
        return colliders.Length > 0;
    }

    public bool GetOccupierOfType<T>(out T obj)
    {
        obj = default(T);
        int layerMask = ~LayerMask.GetMask("Cover");
        Collider[] colliders = Physics.OverlapBox(FloorPosition + 0.5f * Vector3.up * UnitColliderExtents.y, 0.5f * (UnitColliderExtents - Vector3.one * 0.001f), Quaternion.identity, layerMask); // NOTE: shrunk to avoid collision at the edges
        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                if (collider.transform.root.GetComponentInChildren<T>() != null)
                {
                    obj = collider.transform.root.GetComponentInChildren<T>();
                    return true;
                }
            }
        }
        return false;
    }

    public static float EuclideanDistance(GridNode a, GridNode b)
    {
        return Mathf.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));
    }

    public static bool HalfWallBetween(GridNode a, GridNode b)
    {
        Orientations orientation = GetAdjacentDirection(a, b);
        return a.HalfWalls[(int)orientation] || b.HalfWalls[((int)orientation + 2) % 4];
    }

    public static Orientations GetAdjacentDirection(GridNode a, GridNode b)
    {
        if (a.X + 1 == b.X && a.Y == b.Y && a.Z == b.Z)
        {
            return Orientations.East;
        }
        if (a.X == b.X && a.Y == b.Y && a.Z + 1 == b.Z)
        {
            return Orientations.North;
        }
        if (a.X - 1 == b.X && a.Y == b.Y && a.Z == b.Z)
        {
            return Orientations.West;
        }
        if (a.X == b.X && a.Y == b.Y && a.Z - 1 == b.Z)
        {
            return Orientations.South;
        }
        return Orientations.East;
    }

    public static bool AreDiagonals(GridNode a, GridNode b)
    {
        return (Mathf.Abs(a.X - b.X) == 1 && Mathf.Abs(a.Z - b.Z) == 1 && (a.Y == b.Y));
    }
}
