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
    public bool IsWalkable { get; set; } = false;
    public bool IsAir { get; set; } = false;

    public bool Visited { get; set; } = false;
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

    #endregion

    public void Reset()
    {
        Distance = int.MaxValue;
        Visited = false;
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
}
