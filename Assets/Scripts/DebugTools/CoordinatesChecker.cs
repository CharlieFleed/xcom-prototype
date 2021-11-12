using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesChecker : MonoBehaviour
{
    public int X, Y, Z;

    // Update is called once per frame
    void Update()
    {
        GridNode n = GridManager.Instance.GetGridNodeFromWorldPosition(transform.position);
        if (n != null)
        {
            X = n.X;
            Y = n.Y;
            Z = n.Z;
        }
        else
        {
            X = -1;
            Y = -1;
            Z = -1;
        }
    }
}
