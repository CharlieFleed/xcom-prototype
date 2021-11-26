using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeSim : MonoBehaviour
{
    public Vector3 Impulse { get; set; }
    public Vector3 Origin { get; set; }
    public Vector3 Target { get; set; }

    Vector3[] _trajectory;
    int _count = 0;
    public int Count { get { return _count; } }
    Transform _transform;
    public Transform Transform { get { return _transform; } }

    private void Awake()
    {
        _transform = transform;
    }

    public Vector3[] Trajectory
    {
        get
        {
            Vector3[] t = new Vector3[_count];
            Array.Copy(_trajectory, t, Count);
            return t;
        }
    }


    public void InitTrajectory(int numPoints)
    {
        _trajectory = new Vector3[numPoints];
    }

    public void SavePoint()
    {
        _trajectory[_count++] = _transform.position;
    }
}
