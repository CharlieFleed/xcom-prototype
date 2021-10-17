using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridObject : MonoBehaviour
{
    public bool IsWalkable;
    public bool IsLadder;

    public int Y;

    public List<GridNode> floorNodes = new List<GridNode>();
}
