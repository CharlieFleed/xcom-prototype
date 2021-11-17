using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesChecker : MonoBehaviour
{
    public int X, Y, Z;

    public bool IsWalkable = false;
    public bool IsAir = false;
    public bool IsBooked = false;

    // Update is called once per frame
    void Update()
    {
        GridNode n = GridManager.Instance.GetGridNodeFromWorldPosition(transform.position);
        if (n != null)
        {
            X = n.X;
            Y = n.Y;
            Z = n.Z;
            IsWalkable = n.IsWalkable;
            IsAir = n.IsAir;
            IsBooked = n.IsBooked;
        }
        else
        {
            X = -1;
            Y = -1;
            Z = -1;
        }
    }
}
