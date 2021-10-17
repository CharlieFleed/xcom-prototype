using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridCoverManager : MonoBehaviour
{
    private void Awake()
    {
        _instance = this;
    }

    /// <summary>
    /// -1: flanked, 1: half-cover, 2: full-cover
    /// </summary>
    /// <param name="target"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public int GetCoverFromPosition(GridNode target, GridNode position)
    {
        int cover = -1;
        if (position.X > target.X)
        {
            // check the East half-wall
            if (target.HalfWalls[(int)GridNode.Orientations.East])
            {
                cover = Mathf.Max(cover, 1);
            }
            // check the East wall
            if (target.Walls[(int)GridNode.Orientations.East])
            {
                cover = Mathf.Max(cover, 2);
            }
        }
        if (position.X < target.X)
        {
            // check the West half-wall
            if (target.HalfWalls[(int)GridNode.Orientations.West])
            {
                cover = Mathf.Max(cover, 1);
            }
            // check the West wall
            if (target.Walls[(int)GridNode.Orientations.West])
            {
                cover = Mathf.Max(cover, 2);
            }
        }
        if (position.Z > target.Z)
        {
            // check the North half-wall
            if (target.HalfWalls[(int)GridNode.Orientations.North])
            {
                cover = Mathf.Max(cover, 1);
            }
            // check the North wall
            if (target.Walls[(int)GridNode.Orientations.North])
            {
                cover = Mathf.Max(cover, 2);
            }
        }
        if (position.Z < target.Z)
        {
            // check the South half-wall
            if (target.HalfWalls[(int)GridNode.Orientations.South])
            {
                cover = Mathf.Max(cover, 1);
            }
            // check the South wall
            if (target.Walls[(int)GridNode.Orientations.South])
            {
                cover = Mathf.Max(cover, 2);
            }
        }
        return cover;
    }

    public int GetLocalCover(GridNode currentNode)
    {
        int cover = 0;
        for (int i = 0; i < 4; i++)
        {
            // check half-wall
            if (currentNode.HalfWalls[i])
            {
                cover = Mathf.Max(cover, 1);
            }
            // check wall
            if (currentNode.Walls[i])
            {
                cover = Mathf.Max(cover, 2);
            }
        }
        return cover;
    }

    public int GetCover(GridEntity gridEntity, List<GridEntity> enemies)
    {
        int worstCover = 2; // -1: flanked; 1: half cover; 2: full cover;
        bool isSeen = false;
        foreach (GridEntity enemy in enemies)
        {
            List<GridNode[]> losPoints = new List<GridNode[]>();
            bool los = LineOfSight(enemy, gridEntity, out Ray ray, out float rayLength, losPoints);
            if (los)
            {
                isSeen = true;
                foreach (var points in losPoints)
                {
                    int cover = GetCoverFromPosition(gridEntity.CurrentNode, points[0]);
                    worstCover = Mathf.Min(cover, worstCover);
                }
            }
        }
        if (isSeen)
        {
            return worstCover;
        }
        else
        {
            return GetLocalCover(gridEntity.CurrentNode);
        }
    }

    public bool LineOfSight(GridEntity a, GridEntity b, out Ray ray, out float rayLength, List<GridNode[]> losPoints)
    {
        //Debug.Log($"LineOfSight - Shooter: {a.gameObject.name}, Target: {b.gameObject.name}.");
        rayLength = (b.transform.position - a.transform.position).magnitude;
        ray = new Ray(a.transform.position + Vector3.up * 1, b.transform.position - a.transform.position);
        RaycastHit[] hits = Physics.RaycastAll(ray, rayLength, LayerMask.GetMask("Cover"));
        if (hits.Length == 0)
        {
            //Debug.Log($"Direct LOS.");
            losPoints.Add(new GridNode[] { a.CurrentNode, b.CurrentNode });
        }
        List<GridNode> targetSideSteps = SideSteps(b, a.CurrentNode);
        foreach (var targetSideStep in targetSideSteps)
        {
            //Debug.Log($"Checking target sidestep {targetSideStep.X},{targetSideStep.Y},{targetSideStep.Z}.");
            rayLength = (targetSideStep.FloorPosition - a.transform.position).magnitude;
            ray = new Ray(a.transform.position + Vector3.up * 1, targetSideStep.FloorPosition - a.transform.position);
            hits = Physics.RaycastAll(ray, rayLength, LayerMask.GetMask("Cover"));
            if (hits.Length == 0)
            {
                //Debug.Log($"Direct LOS.");
                losPoints.Add(new GridNode[] { a.CurrentNode, targetSideStep });
            }
        }
        //Debug.Log($"Direct LOS Collision.");
        // side stepping
        List<GridNode> sideSteps = SideSteps(a, b.CurrentNode);
        //Debug.Log($"# of shooter sidesteps: {sideSteps.Count}");
        foreach (GridNode sideStep in sideSteps)
        {
            //Debug.Log($"Checking shooter side-step: {sideStep.X},{sideStep.Y},{sideStep.Z}.");
            rayLength = (b.transform.position - sideStep.FloorPosition).magnitude;
            ray = new Ray(sideStep.FloorPosition + Vector3.up * 1, b.transform.position - sideStep.FloorPosition);
            hits = Physics.RaycastAll(ray, rayLength, LayerMask.GetMask("Cover"));
            if (hits.Length == 0)
            {
                //Debug.Log($"Side-step LOS.");
                losPoints.Add(new GridNode[] { sideStep, b.CurrentNode });
            }
            foreach (var targetSideStep in targetSideSteps)
            {
                //Debug.Log($"Checking target sidestep {targetSideStep.X},{targetSideStep.Y},{targetSideStep.Z}.");
                rayLength = (targetSideStep.FloorPosition - sideStep.FloorPosition).magnitude;
                ray = new Ray(sideStep.FloorPosition + Vector3.up * 1, targetSideStep.FloorPosition - sideStep.FloorPosition);
                hits = Physics.RaycastAll(ray, rayLength, LayerMask.GetMask("Cover"));
                if (hits.Length == 0)
                {
                    //Debug.Log($"Direct LOS.");
                    losPoints.Add(new GridNode[] { sideStep, targetSideStep });
                    return true;
                }
            }
            //Debug.Log($"Collision.");
        }
        return losPoints.Count > 0;
    }

    public List<GridNode> SideSteps(GridEntity gridEntity, GridNode targetNode)
    {
        List<GridNode> sideSteps = new List<GridNode>();
        GridNode node = gridEntity.CurrentNode;
        //Debug.Log($"Sidesteps for {agent.name} in position: {node.X},{node.Y},{node.Z}, target: {target.X},{target.Y},{target.Z}.");
        GridAgent gridAgent = gridEntity.GetComponent<GridAgent>();
        if (gridAgent)
        {
            List<GridNode> neighbors = GridManager.Instance.GetXZNeighbors(node, gridAgent);
            //Debug.Log($"Neighbors:");
            //foreach (var neighbor in neighbors)
            //{
            //    Debug.Log($"{neighbor.X},{neighbor.Y},{neighbor.Z}.");
            //}
            GridNode east = GridManager.Instance.GetAdjacent(node, GridNode.Orientations.East);
            GridNode north = GridManager.Instance.GetAdjacent(node, GridNode.Orientations.North);
            GridNode west = GridManager.Instance.GetAdjacent(node, GridNode.Orientations.West);
            GridNode south = GridManager.Instance.GetAdjacent(node, GridNode.Orientations.South);
            // check the North wall for the East and West sidesteps
            if (targetNode.Z > node.Z && (node.HalfWalls[(int)GridNode.Orientations.North] || node.Walls[(int)GridNode.Orientations.North]))
            {
                if (east != null && neighbors.Contains(east) && !sideSteps.Contains(east))
                {
                    sideSteps.Add(east);
                }
                if (west != null && neighbors.Contains(west) && !sideSteps.Contains(west))
                {
                    sideSteps.Add(west);
                }
            }
            // check the South wall for the East and West sidesteps
            if (targetNode.Z < node.Z && (node.HalfWalls[(int)GridNode.Orientations.South] || node.Walls[(int)GridNode.Orientations.South]))
            {
                if (east != null && neighbors.Contains(east) && !sideSteps.Contains(east))
                {
                    sideSteps.Add(east);
                }
                if (west != null && neighbors.Contains(west) && !sideSteps.Contains(west))
                {
                    sideSteps.Add(west);
                }
            }
            // check the East wall for the North and South sidesteps
            if (targetNode.X > node.X && (node.HalfWalls[(int)GridNode.Orientations.East] || node.Walls[(int)GridNode.Orientations.East]))
            {
                if (north != null && neighbors.Contains(north) && !sideSteps.Contains(north))
                {
                    sideSteps.Add(north);
                }
                if (south != null && neighbors.Contains(south) && !sideSteps.Contains(south))
                {
                    sideSteps.Add(south);
                }
            }
            // check the West wall for the North and South sidesteps
            if (targetNode.X < node.X && (node.HalfWalls[(int)GridNode.Orientations.West] || node.Walls[(int)GridNode.Orientations.West]))
            {
                if (north != null && neighbors.Contains(north) && !sideSteps.Contains(north))
                {
                    sideSteps.Add(north);
                }
                if (south != null && neighbors.Contains(south) && !sideSteps.Contains(south))
                {
                    sideSteps.Add(south);
                }
            }
        }
        return sideSteps;
    }

    public List<ShotStats> GetShotStats(BattleAction battleAction, List<GridEntity> enemies)
    {
        List<ShotStats> targets = new List<ShotStats>();
        foreach (GridEntity enemy in enemies)
        {
            List<GridNode[]> losPoints = new List<GridNode[]>();
            bool los = LineOfSight(battleAction.GetComponent<GridEntity>(), enemy, out Ray ray, out float rayLength, losPoints);
            if (los)
            {
                ShotStats target = new ShotStats();
                target.Target = enemy.GetComponent<GridEntity>();
                target.Available = true;
                int worstCover = 2; // -1: flanked; 1: half cover; 2: full cover;
                foreach (var points in losPoints)
                {
                    int cover = GetCoverFromPosition(enemy.CurrentNode, points[0]);
                    worstCover = Mathf.Min(cover, worstCover);
                }
                if (worstCover == -1)
                {
                    target.Flanked = true;
                }
                if (worstCover == 1)
                {
                    target.HalfCover = true;
                }
                if (worstCover == 2)
                {
                    target.Cover = true;
                }
                targets.Add(target);
            }
        }
        return targets;
    }

    #region Singleton

    private static GridCoverManager _instance;

    public static GridCoverManager Instance { get { return _instance; } }

    #endregion
}
