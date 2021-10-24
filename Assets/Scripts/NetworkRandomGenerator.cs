using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkRandomGenerator : NetworkBehaviour
{
    readonly SyncList<int> _ints = new SyncList<int>();

    int _next;

    private void Awake()
    {
        _instance = this;
    }

    public int RandomRange(int min, int max)
    {
        if (isServer)
        {
            int value = Random.Range(min, max);
            _ints.Add(value);
            return value;
        }
        else
        {
            int value = _ints[_next];
            _next++;
            return value;
        }
    }

    public bool Ready()
    {
        if (isServer)
        {
            return true;
        }
        else
        {
            return _next < _ints.Count;
        }
    }

    #region Singleton

    private static NetworkRandomGenerator _instance;

    public static NetworkRandomGenerator Instance { get { return _instance; } }

    #endregion
}
