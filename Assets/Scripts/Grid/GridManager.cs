using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GridManager : MonoBehaviour
{
    #region Fields

    [SerializeField] private float _xzScale = 2;
    [SerializeField] private float _yScale = 4;
    [SerializeField] Vector3 _unitColliderExtents = new Vector3(1, 2, 1);
    [SerializeField] Vector3 _tileExtents = new Vector3(1, 2, 1);

    [SerializeField] LayerMask _floorLayerMask;

    [SerializeField] bool _drawBoxes = false;
    [SerializeField] bool _drawUnitColliders = false;
    [SerializeField] bool _drawFloor = false;
    [SerializeField] bool _drawWalls = false;
    [SerializeField] bool _drawBalls = false;
    [SerializeField] bool _drawAir = false;


    Vector3 _origin;
    Grid _grid;
    int _gridSizeX;
    int _gridSizeY;
    int _gridSizeZ;

    Vector3 _gridBoxColliderExtents;

    List<Vector3> _wireCubesPositions = new List<Vector3>();

    List<Vector3> _wallPositions = new List<Vector3>();
    List<Vector3> _wallExtents = new List<Vector3>();

    public float XZScale { get { return _xzScale; } }
    public float YScale { get { return _yScale; } }
    public Vector3 UnitColliderExtents { get { return _unitColliderExtents; } }
    public Vector3 TileExtents { get { return _tileExtents; } }

    #endregion

    private void Awake()
    {
        _instance = this;
        _gridBoxColliderExtents = new Vector3(_xzScale, _yScale, _xzScale);
        GetGridSizes();
        GenerateGrid();
        GenerateWalls();
        GenerateCovers();
        CheckLadders();
    }

    void GetGridSizes()
    {
        GridPivot[] gridPivots = GameObject.FindObjectsOfType<GridPivot>();

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        foreach (var pivot in gridPivots)
        {
            Transform t = pivot.transform;
            if (t.position.x < minX)
            {
                minX = t.position.x;
            }
            if (t.position.y < minY)
            {
                minY = t.position.y;
            }
            if (t.position.z < minZ)
            {
                minZ = t.position.z;
            }

            if (t.position.x > maxX)
            {
                maxX = t.position.x;
            }
            if (t.position.y > maxY)
            {
                maxY = t.position.y;
            }
            if (t.position.z > maxZ)
            {
                maxZ = t.position.z;
            }
        }

        _gridSizeX = 1 + Mathf.FloorToInt((maxX - minX) / _xzScale);
        _gridSizeY = 1 + Mathf.FloorToInt((maxY - minY) / _yScale);
        _gridSizeZ = 1 + Mathf.FloorToInt((maxZ - minZ) / _xzScale);

        _origin = new Vector3(minX, minY, minZ);
    }

    void GenerateGrid()
    {
        _grid = new Grid(_gridSizeX, _gridSizeY, _gridSizeZ);

        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int z = 0; z < _gridSizeZ; z++)
                {
                    GridNode gridNode = new GridNode() { X = x, Y = y, Z = z };
                    gridNode.UnitColliderExtents = _unitColliderExtents;

                    Vector3 position = _origin;
                    position.x += x * _xzScale;
                    position.y += y * _yScale;
                    position.z += z * _xzScale;
                    gridNode.WorldPosition = position;

                    _grid.SetNode(x, y, z, gridNode);
                    //Debug.Log($"creating node: {x},{y},{z}");

                    // Find any GridObjects in the volume of this cell.
                    Collider[] colliders = Physics.OverlapBox(gridNode.WorldPosition, 0.5f * (_gridBoxColliderExtents - 0.001f * new Vector3(1, 0, 1))); // NOTE: shrunk to avoid collision at the sides
                    if (colliders.Length > 0)
                    {
                        _wireCubesPositions.Add(position); // there is something in this volume
                        foreach (var collider in colliders)
                        {
                            GridObject obj = collider.GetComponentInChildren<GridObject>();
                            if (obj != null) // only consider GridObjects
                            {
                                obj.Y = Mathf.Max(y, obj.Y);
                                if (obj.IsWalkable) // collides with floor
                                {
                                    obj.floorNodes.Add(gridNode);
                                    //
                                    gridNode.HasFloor = true;
                                    gridNode.IsWalkable = true; // walkable node candidate
                                }
                            }
                        }
                    }

                    //FindFloorLevel(gridNode);
                    FindFloorLevelWithBoxCast(gridNode);
                    CheckRoomForUnit(gridNode);
                    //CheckRoomForUnit2(gridNode);

                    // check if air
                    if (!gridNode.IsWalkable)
                    {
                        gridNode.IsAir = true;
                        //Collider[] airColliders = Physics.OverlapBox(gridNode.WorldPosition + 0.5f * Vector3.up * (_yScale), 0.5f * (new Vector3(_unitColliderExtents.x, _yScale, _unitColliderExtents.z) - Vector3.one * 0.001f));
                        Collider[] airColliders = Physics.OverlapBox(gridNode.WorldPosition, 0.5f * (new Vector3(_unitColliderExtents.x, _yScale, _unitColliderExtents.z) - Vector3.one * 0.001f));
                        if (airColliders.Length > 0)
                        {
                            foreach (var airCollider in airColliders)
                            {
                                //Debug.Log($"Collision with {airCollider}");
                                GridObject obj = airCollider.GetComponentInChildren<GridObject>();
                                if (obj != null) // only consider GridObjects
                                {
                                    gridNode.IsAir = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void FindFloorLevelWithRayCast(GridNode gridNode)
    {
        // Find the floor level.
        gridNode.FloorPosition = gridNode.WorldPosition - Vector3.up * 0.5f * _yScale;
        if (gridNode.HasFloor)
        {
            RaycastHit hit;
            // start from the top of the box
            Vector3 origin = gridNode.WorldPosition + Vector3.up * 0.5f * _yScale;
            if (Physics.Raycast(origin, Vector3.down, out hit, _yScale, _floorLayerMask)) // only look for floor
            {
                gridNode.FloorPosition = hit.point;
            }
            else
            {
                gridNode.HasFloor = false;
                gridNode.IsWalkable = false;
            }
        }
    }

    private void FindFloorLevelWithBoxCast(GridNode gridNode)
    {
        // Find the floor level.
        gridNode.FloorPosition = gridNode.WorldPosition - Vector3.up * 0.5f * _yScale;
        if (gridNode.HasFloor)
        {
            RaycastHit hit;
            // start from the top of the box
            Vector3 origin = gridNode.WorldPosition + Vector3.up * 0.5f * _yScale;
            //Debug.Log($"{origin.x},{origin.y},{origin.z}");
            if (Physics.BoxCast(origin, new Vector3(0.5f * _unitColliderExtents.x, 0.001f, 0.5f * _unitColliderExtents.z), Vector3.down, out hit, Quaternion.identity, _yScale, _floorLayerMask)) // only look for floor
            {
                //Debug.Log($"Floor at y={hit.point.y}!!!");
                gridNode.FloorPosition = new Vector3(gridNode.FloorPosition.x, hit.point.y, gridNode.FloorPosition.z);
            }
            else
            {
                gridNode.HasFloor = false;
                gridNode.IsWalkable = false;
            }
        }
    }

    private void CheckRoomForUnit2(GridNode gridNode)
    {
        // Check room for unit second method
        if (gridNode.IsWalkable)
        {
            float floorOffset = 0;
            bool roomForUnit = true;
            Debug.Log($"({gridNode.X},{gridNode.Y},{gridNode.Z}) gridNode.FloorPosition.y:{gridNode.FloorPosition.y}, gridNode.WorldPosition.y:{gridNode.WorldPosition.y}");
            while (gridNode.FloorPosition.y + floorOffset <= gridNode.WorldPosition.y + 0.5f * _yScale)
            {
                Collider[] unitColliders = Physics.OverlapBox(gridNode.FloorPosition + Vector3.up * floorOffset + 0.5f * Vector3.up * (_unitColliderExtents.y), 0.5f * (_unitColliderExtents - Vector3.one * 0.001f)); // NOTE: shrunk to avoid collision at the edges
                roomForUnit = true;
                if (unitColliders.Length > 0)
                {
                    foreach (var unitCollider in unitColliders)
                    {
                        GridObject obj = unitCollider.GetComponentInChildren<GridObject>();
                        if (obj != null) // only consider GridObjects
                        {
                            if (!obj.IsWalkable)
                            {
                                gridNode.IsWalkable = false;
                                return;
                            }
                            else
                            {
                                roomForUnit = false;
                                break;
                            }
                        }
                    }
                }
                if (roomForUnit)
                {
                    break;
                }
                floorOffset += 0.1f;
            }
            if (!roomForUnit)
            {
                gridNode.IsWalkable = false;
            }
            else
            {
                gridNode.FloorPosition = gridNode.FloorPosition + Vector3.up * floorOffset;
            }
        }
    }

    private void CheckRoomForUnit(GridNode gridNode)
    {
        // Check room for unit first method
        if (gridNode.IsWalkable)
        {
            LayerMask layerMask = ~LayerMask.GetMask("Destructibles");
            Collider[] unitColliders = Physics.OverlapBox(gridNode.FloorPosition + 0.5f * Vector3.up * (_unitColliderExtents.y), 0.5f * (_unitColliderExtents - Vector3.one * 0.001f), Quaternion.identity, layerMask); // NOTE: shrunk to avoid collision at the edges
            if (unitColliders.Length > 0)
            {
                foreach (var unitCollider in unitColliders)
                {
                    GridObject obj = unitCollider.GetComponentInChildren<GridObject>();
                    if (obj != null) // only consider GridObjects
                    {
                        // collision with some grid object, there is no room for a unit
                        //Debug.Log($"Collision with {obj}");
                        gridNode.IsWalkable = false;
                        break;
                    }
                }
            }
            if (gridNode.IsWalkable && IsInsideMesh(gridNode.FloorPosition + 0.5f * Vector3.up * (_unitColliderExtents.y)))
            {
                gridNode.IsWalkable = false;
            }
        }
    }

    private bool IsInsideMesh(Vector3 point)
    {
        Physics.queriesHitBackfaces = true;
        RaycastHit[] hitsUp = Physics.RaycastAll(point, Vector3.up);
        RaycastHit[] hitsDown = Physics.RaycastAll(point, Vector3.down);
        Physics.queriesHitBackfaces = false;
        for (var i = 0; i < hitsUp.Length; i++)
            if (hitsUp[i].normal.y > 0)
                for (var j = 0; j < hitsDown.Length; j++)
                    if (hitsDown[j].normal.y < 0 && hitsDown[j].collider == hitsUp[i].collider)
                        return true;

        return false;
    }

    void GenerateWalls()
    {
        foreach (GridNode gridNode in _grid.Nodes())
        {
            //Debug.Log($"GenerateWalls node: {gridNode.X},{gridNode.Y},{gridNode.Z}");
            // check walls and half walls
            Vector3[] wallTopOffsets = new Vector3[4];
            wallTopOffsets[0] = (_unitColliderExtents.y - 0.001f) * Vector3.up + 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.right; // NOTE: lowered to avoid collision with a _unitColliderExtents.y-tall doorway  
            wallTopOffsets[1] = (_unitColliderExtents.y - 0.001f) * Vector3.up + 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.forward;
            wallTopOffsets[2] = (_unitColliderExtents.y - 0.001f) * Vector3.up + 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.left;
            wallTopOffsets[3] = (_unitColliderExtents.y - 0.001f) * Vector3.up + 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.back;
            //
            Vector3[] wallTopHalfExtents = new Vector3[4];
            // NOTE: shrunk to avoid collision with a _unitColliderExtents.z-wide doorway, extended to reliably hit colliders at the edge of the node
            wallTopHalfExtents[0] = new Vector3(0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.x), 0, 0.5f * (_unitColliderExtents.z - 0.001f)) + 0.001f * Vector3.right;
            wallTopHalfExtents[1] = new Vector3(0.5f * (_unitColliderExtents.x - 0.001f), 0, 0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.z)) + 0.001f * Vector3.forward;
            wallTopHalfExtents[2] = new Vector3(0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.x), 0, 0.5f * (_unitColliderExtents.z - 0.001f)) + 0.001f * Vector3.right;
            wallTopHalfExtents[3] = new Vector3(0.5f * (_unitColliderExtents.x - 0.001f), 0, 0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.z)) + 0.001f * Vector3.forward;
            for (int i = 0; i < 4; i++)
            {
                Physics.queriesHitBackfaces = true;
                RaycastHit[] hits = Physics.BoxCastAll(gridNode.FloorPosition + wallTopOffsets[i], wallTopHalfExtents[i], Vector3.down, Quaternion.identity, _unitColliderExtents.y - 0.001f);
                //Debug.Log($"center: {wallTopHalfExtents[i].x},{wallTopHalfExtents[i].y},{wallTopHalfExtents[i].z}");
                //Debug.Log($"halfExtents: {(gridNode.FloorPosition + wallTopOffsets[i]).x},{(gridNode.FloorPosition + wallTopOffsets[i]).y},{(gridNode.FloorPosition + wallTopOffsets[i]).z}");

                Physics.queriesHitBackfaces = false;
                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        GridObject obj = hit.collider.GetComponentInChildren<GridObject>();
                        if (obj != null) // only consider GridObjects
                        {
                            if (hit.distance == 0)
                            {
                                gridNode.Walls[i] = true;
                                //Debug.Log($"Wall {i}");
                                gridNode.HalfWalls[i] = false;
                                break; // full wall, that's it
                            }
                            else if (hit.distance <= 0.5f * _unitColliderExtents.y)
                            {
                                gridNode.HalfWalls[i] = true;
                                //Debug.Log($"HalfWall {i}");
                            }
                        }
                    }
                }
                if (!gridNode.Walls[i] && !gridNode.HalfWalls[i])
                {
                    if (IsInsideMesh(gridNode.FloorPosition + wallTopOffsets[i] - (0.5f * _unitColliderExtents.y - 0.001f) * Vector3.up))
                    {
                        gridNode.Walls[i] = true;
                    }
                }
            }

            // walls visualization
            Vector3[] wallOffsets = new Vector3[4];
            wallOffsets[0] = Vector3.right * 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) + Vector3.up * 0.5f * _unitColliderExtents.y;
            wallOffsets[1] = Vector3.forward * 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) + Vector3.up * 0.5f * _unitColliderExtents.y;
            wallOffsets[2] = Vector3.left * 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) + Vector3.up * 0.5f * _unitColliderExtents.y;
            wallOffsets[3] = Vector3.back * 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) + Vector3.up * 0.5f * _unitColliderExtents.y;
            Vector3[] wallHalfExtents = new Vector3[4];
            wallHalfExtents[0] = new Vector3(0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.x), 0.5f * (_unitColliderExtents.y), 0.5f * _unitColliderExtents.z);
            wallHalfExtents[1] = new Vector3(0.5f * _unitColliderExtents.x, 0.5f * (_unitColliderExtents.y), 0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.z));
            wallHalfExtents[2] = new Vector3(0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.x), 0.5f * (_unitColliderExtents.y), 0.5f * _unitColliderExtents.z);
            wallHalfExtents[3] = new Vector3(0.5f * _unitColliderExtents.x, 0.5f * (_unitColliderExtents.y), 0.5f * (0.5f * _xzScale - 0.5f * _unitColliderExtents.z));
            for (int i = 0; i < 4; i++)
            {
                if (gridNode.Walls[i])
                {
                    _wallPositions.Add(gridNode.FloorPosition + wallOffsets[i]);
                    _wallExtents.Add(2 * wallHalfExtents[i]);
                }
                else if (gridNode.HalfWalls[i])
                {
                    _wallPositions.Add(gridNode.FloorPosition + wallOffsets[i] - Vector3.up * 0.25f * _unitColliderExtents.y);
                    _wallExtents.Add(2 * (wallHalfExtents[i] - Vector3.up * 0.5f * wallHalfExtents[i].y));
                }
            }
        }
    }

    void GenerateCovers()
    {
        foreach (GridNode gridNode in _grid.Nodes())
        {
            // cover
            Vector3[] coverOffsets = new Vector3[4];
            coverOffsets[0] = Vector3.right * 0.5f * _xzScale + Vector3.up * 0.5f * _unitColliderExtents.y;
            coverOffsets[1] = Vector3.forward * 0.5f * _xzScale + Vector3.up * 0.5f * _unitColliderExtents.y;
            coverOffsets[2] = Vector3.left * 0.5f * _xzScale + Vector3.up * 0.5f * _unitColliderExtents.y;
            coverOffsets[3] = Vector3.back * 0.5f * _xzScale + Vector3.up * 0.5f * _unitColliderExtents.y;
            for (int i = 0; i < 4; i++)
            {
                if (gridNode.Walls[i])
                {
                    // cover objects for LOS
                    GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    cover.transform.localScale = new Vector3(_xzScale, _unitColliderExtents.y, _xzScale);
                    cover.transform.rotation = Quaternion.Euler(0, 90 * (i + 1), 0);
                    cover.transform.position = gridNode.FloorPosition + coverOffsets[i];
                    cover.GetComponent<MeshRenderer>().enabled = false;
                    cover.layer = 7;
                    cover.transform.SetParent(GameObject.Find("Covers").transform);
                    GameObject cover2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    cover2.transform.localScale = new Vector3(_xzScale, _unitColliderExtents.y, _xzScale);
                    cover2.transform.rotation = Quaternion.Euler(0, -90 * (i + 1), 0);
                    cover2.transform.position = gridNode.FloorPosition + coverOffsets[i];
                    cover2.GetComponent<MeshRenderer>().enabled = false;
                    cover2.layer = 7;
                    cover2.transform.SetParent(GameObject.Find("Covers").transform);
                }
            }
            if (gridNode.HasFloor)
            {
                GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cover.transform.localScale = new Vector3(_xzScale, _xzScale, _xzScale);
                cover.transform.rotation = Quaternion.Euler(90, 0, 0);
                cover.transform.position = gridNode.FloorPosition;
                cover.GetComponent<MeshRenderer>().enabled = false;
                cover.layer = 7;
                cover.transform.SetParent(GameObject.Find("Covers").transform);
                GameObject cover2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cover2.transform.localScale = new Vector3(_xzScale, _xzScale, _xzScale);
                cover2.transform.rotation = Quaternion.Euler(-90, 0, 0);
                cover2.transform.position = gridNode.FloorPosition;
                cover2.GetComponent<MeshRenderer>().enabled = false;
                cover2.layer = 7;
                cover2.transform.SetParent(GameObject.Find("Covers").transform);
            }
        }
    }

    void CheckLadders()
    {
        Dictionary<GridObject, List<GridNode>> ladders = new Dictionary<GridObject, List<GridNode>>();
        foreach (GridNode gridNode in _grid.Nodes())
        {
            Vector3[] ladderOffsets = new Vector3[4];
            ladderOffsets[0] = 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.right;
            ladderOffsets[1] = 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.forward;
            ladderOffsets[2] = 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.left;
            ladderOffsets[3] = 0.5f * (0.5f * _unitColliderExtents.x + 0.5f * _xzScale) * Vector3.back;
            Vector3 ladderHalfExtents = 0.5f * new Vector3(
                0.5f * _xzScale - 0.5f * _unitColliderExtents.x - 0.001f, 
                _yScale - 0.001f, 
                0.5f * _xzScale - 0.5f * _unitColliderExtents.z - 0.001f);
            for (int i = 0; i < 4; i++)
            {
                Collider[] colliders = Physics.OverlapBox(gridNode.WorldPosition + ladderOffsets[i], ladderHalfExtents);
                if (colliders.Length > 0)
                {
                    foreach (var collider in colliders)
                    {
                        GridObject obj = collider.GetComponentInChildren<GridObject>();
                        if (obj != null) // only consider GridObjects
                        {
                            if (obj.IsLadder)
                            {
                                gridNode.Ladders[i] = true;
                                //Debug.Log($"Ladder {obj.name} {(GridNode.Orientations)i} of {gridNode.X},{gridNode.Y},{gridNode.Z}.");
                                if (ladders.ContainsKey(obj))
                                {
                                    ladders[obj].Add(gridNode);
                                }
                                else
                                {
                                    ladders.Add(obj, new List<GridNode>() { gridNode });
                                }
                            }
                        }
                    }
                }
            }
        }
        foreach (var nodesList in ladders.Values)
        {
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            GridNode ladderBase = null;
            GridNode ladderTop = null;
            foreach (var item in nodesList)
            {
                if (item.Y < minY)
                {
                    minY = item.Y;
                    ladderBase = item;
                }
                if (item.Y > maxY)
                {
                    maxY = item.Y;
                    ladderTop = item;
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (ladderBase.Ladders[i])
                {
                    GridNode ladderNeighbor = GetAdjacent(ladderTop, (GridNode.Orientations)i);
                    ladderBase.LadderNeighbors.Add(ladderNeighbor);
                    ladderNeighbor.LadderNeighbors.Add(ladderBase);
                    //Debug.Log($"{ladderBase.X},{ladderBase.Y},{ladderBase.Z} and {ladderNeighbor.X},{ladderNeighbor.Y},{ladderNeighbor.Z} are connected by a ladder.");
                }
            }
        }
    }

    [Obsolete]
    public void PositionPlayers(Queue<Team> teams)
    {
        int count = 0;
        foreach (Team team in teams)
        {
            count += team.Members.Count;
        }
        List<GridNode> walkableNodes = new List<GridNode>();
        // repeat until count walkable nodes are found, stop at 1000 attempts;
        int attemptsLeft = 1000;
        while (count > 0 && attemptsLeft > 0)
        {
            GridNode node = GetGridNode(UnityEngine.Random.Range(0, _gridSizeX), UnityEngine.Random.Range(0, _gridSizeY), UnityEngine.Random.Range(0, _gridSizeZ));
            if (!walkableNodes.Contains(node) && node.IsWalkable && !node.IsOccupied())
            {
                walkableNodes.Add(node);
                count--;
            }
            else
            {
                attemptsLeft--;
            }
        }
        if (count == 0)
        {
            foreach (Team team in teams)
            {
                foreach (TeamMember member in team.Members)
                {
                    member.gameObject.transform.position = walkableNodes[count].FloorPosition;
                    Debug.Log("Set Current Node");
                    member.gameObject.GetComponent<GridEntity>().CurrentNode = walkableNodes[count];
                    count++;
                }
            }
        }
        else
        {
            Debug.Log("Not enough walkable nodes for units. Exiting.");
            Application.Quit();
        }
    }

    public List<GridNode> GetSpawnPositions(int teamId, int count)
    {
        GameObject spawnersGroup = GameObject.Find("Spawners " + teamId);
        List<GridNode> spawnPoints = new List<GridNode>();
        foreach (Transform spawnPoint in spawnersGroup.transform)
        {
            spawnPoints.Add(GetGridNodeFromWorldPosition(spawnPoint.position));
            if (spawnPoints.Count == count)
            {
                break;
            }
        }
        return spawnPoints;
    }

    [Obsolete]
    public List<GridNode> GetSpawnPositionsOld(int teamId, int count)
    {
        GameObject spawnersGroup = GameObject.Find("Spawners " + teamId);
        List<GridNode> spawnPoints = new List<GridNode>();
        foreach (Transform spawnPoint in spawnersGroup.transform)
        {
            spawnPoints.Add(GetGridNodeFromWorldPosition(spawnPoint.position));
        }

        List<GridNode> walkableNodes = new List<GridNode>();
        // repeat until count walkable nodes are found, stop at 1000 attempts;
        int attemptsLeft = 1000;
        while (count > 0 && attemptsLeft > 0)
        {
            GridNode node = GetGridNode(UnityEngine.Random.Range(0, _gridSizeX), UnityEngine.Random.Range(0, _gridSizeY), UnityEngine.Random.Range(0, _gridSizeZ));
            if (!walkableNodes.Contains(node) && node.IsWalkable && !node.IsOccupied())
            {
                walkableNodes.Add(node);
                count--;
            }
            else
            {
                attemptsLeft--;
            }
        }
        if (count == 0)
        {
            return walkableNodes;
        }
        else
        {
            Debug.Log("Not enough walkable nodes for units. Exiting.");
            Application.Quit();
            return null;
        }
    }

    public GridNode GetGridNodeFromWorldPosition(Vector3 wp)
    {
        Vector3 p = wp - _origin;
        int x = Mathf.FloorToInt((0.5f * _xzScale + p.x) / _xzScale);
        int y = Mathf.FloorToInt((0.5f * _yScale + p.y) / _yScale);
        int z = Mathf.FloorToInt((0.5f * _xzScale + p.z) / _xzScale);
        return GetGridNode(x, y, z);
    }

    public GridNode GetGridNode(int x, int y, int z)
    {
        return _grid.Node(x, y, z);
    }

    public Grid GetGrid()
    {
        return _grid;
    }

    // NOTE: this is partially duplicated in Pathfinder
    public List<GridNode> GetXZNeighbors(GridNode node, GridAgent gridAgent)
    {
        List<GridNode> neighbors = new List<GridNode>();
        int[][] xzOffsets = { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
        for (int i = 0; i < xzOffsets.Length; i++)
        {
            // stop if there is a wall
            if (node.Walls[i])
            {
                continue;
            }
            GridNode neighbor = GetGridNode(node.X + xzOffsets[i][0], node.Y, node.Z + xzOffsets[i][1]);
            // check if the candidate point belongs to the map
            if (neighbor != null)
            {
                //Debug.Log($"GetXZNeighbors neighbor: {neighbor.X},{neighbor.Y},{neighbor.Z}.");
                // check if walkable
                if (!neighbor.IsWalkable)
                {
                    //Debug.Log($"Not walkable.");
                    continue;
                }
                // check height difference
                if (node.FloorPosition.y - neighbor.FloorPosition.y > gridAgent.MaxJumpDown || neighbor.FloorPosition.y - node.FloorPosition.y > gridAgent.MaxJumpUp)
                {
                    //Debug.Log($"Height difference.");
                    continue;
                }
                // check if available
                if (!IsNodeAvailable(neighbor, gridAgent))
                {
                    //Debug.Log($"Not available.");
                    continue;
                }
                // skip if node above neighbor doesn't leave room
                if ((node.Y + 1) < _gridSizeY)
                {
                    GridNode nodeAboveNeighbor = GetGridNode(node.X + xzOffsets[i][0], node.Y + 1, node.Z + xzOffsets[i][1]);
                    if (nodeAboveNeighbor.HasFloor && nodeAboveNeighbor.FloorPosition.y - node.FloorPosition.y < GridManager.Instance.UnitColliderExtents.y)
                    {
                        continue;
                    }
                }
                //Debug.Log($"OK.");
                neighbors.Add(GetGridNode(node.X + xzOffsets[i][0], node.Y, node.Z + xzOffsets[i][1]));
            }
        }
        return neighbors;
    }

    public GridNode GetAdjacent(GridNode node, GridNode.Orientations direction)
    {
        switch (direction)
        {
            case GridNode.Orientations.East:
                return GetGridNode(node.X + 1, node.Y, node.Z);
            case GridNode.Orientations.North:
                return GetGridNode(node.X, node.Y, node.Z + 1);
            case GridNode.Orientations.West:
                return GetGridNode(node.X - 1, node.Y, node.Z);
            case GridNode.Orientations.South:
                return GetGridNode(node.X, node.Y, node.Z - 1);
            default:
                return null;
        }
    }

    public bool IsNodeAvailable(GridNode node, GridAgent gridAgent)
    {
        //Debug.Log($"Node {node.X},{node.Y},{node.Z}.");
        if (node.IsBooked)
        {
            //Debug.Log($"Node is booked.");
            return false;
        }
        else
        {
            if (node.IsOccupied())
            {
                GridAgent occupier;
                if (node.GetOccupierOfType<GridAgent>(out occupier))
                {
                    if (occupier == gridAgent)
                    {
                        //Debug.Log("It's just me.");
                        return true; // it's just me
                    }
                    else if (occupier.GetComponent<Walker>().IsActive)
                    {
                        return true; // it's someone else moving
                    }
                    else
                    {
                        //Debug.Log("It's someone else.");
                        return false; // it's someone else
                    }
                }
                else // occupant is not a GridAgent, then it is considered an obstacle
                {
                    //Debug.Log("Not a grid agent.");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_grid == null)
            return;
        if (_drawBoxes)
        {
            Gizmos.color = Color.magenta;
            foreach (var wireCubePosition in _wireCubesPositions)
            {
                Gizmos.DrawWireCube(wireCubePosition, _gridBoxColliderExtents);
            }
        }
        if (_drawFloor)
        {
            foreach (var node in _grid.Nodes())
            {
                if (node.HasFloor)
                {
                    if (node.IsWalkable)
                    {
                        Gizmos.color = new Color(0, 1, 0, 0.2f);
                        Gizmos.DrawCube(node.FloorPosition + Vector3.up * 0.1f, new Vector3(_xzScale, 0.2f, _xzScale));
                    }
                    else
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.2f);
                        Gizmos.DrawCube(node.FloorPosition + Vector3.up * 0.1f, new Vector3(_xzScale, 0.2f, _xzScale));
                    }
                }
            }
        }
        if (_drawUnitColliders)
        {
            foreach (var node in _grid.Nodes())
            {
                if (node.HasFloor)
                {
                    if (node.IsWalkable)
                    {
                        Gizmos.color = new Color(0, 1, 0, 0.2f);
                        Gizmos.DrawWireCube(node.FloorPosition + 0.5f * Vector3.up * _unitColliderExtents.y, _unitColliderExtents - Vector3.one * 0.001f);
                    }
                    else
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.2f);
                        Gizmos.DrawWireCube(node.FloorPosition + 0.5f * Vector3.up * _unitColliderExtents.y, _unitColliderExtents - Vector3.one * 0.001f);
                    }
                }
            }
        }
        if (_drawWalls)
        {
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            for (int i = 0; i < _wallPositions.Count; i++)
            {
                Gizmos.DrawWireCube(_wallPositions[i], _wallExtents[i]);
            }
        }
        if (_drawBalls)
        {
            foreach (var node in _grid.Nodes())
            {
                if (node.IsWalkable)
                {
                    if (node.IsOccupied())
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.2f);
                        Gizmos.DrawWireSphere(node.FloorPosition + Vector3.up, 1);
                    }
                    else
                    {
                        Gizmos.color = new Color(0, 1, 0, 0.2f);
                        Gizmos.DrawWireSphere(node.FloorPosition + Vector3.up, 1);
                    }
                }
            }
        }
        if (_drawAir)
        {
            foreach (var node in _grid.Nodes())
            {
                if (node.IsAir)
                {
                        Gizmos.color = new Color(0, 0, 1, 0.2f);
                        Gizmos.DrawWireCube(node.WorldPosition, new Vector3(_unitColliderExtents.x, _yScale, _unitColliderExtents.z) - Vector3.one * 0.001f);
                }
            }
        }
    }

    #region Singleton

    private static GridManager _instance;

    public static GridManager Instance { get { return _instance; } }

    #endregion
}
