using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkRandomGenerator : NetworkBehaviour
{
    readonly SyncList<int> _ints = new SyncList<int>();
    readonly SyncList<int> _batches = new SyncList<int>();
    int _batchCount;

    int _next;
    int _currentBatch;
    int _usedInCurrentBatch;

    private void Awake()
    {
        _instance = this;
    }

    public int RandomRange(int minInclusive, int maxExclusive)
    {
        if (isServer)
        {
            int value = Random.Range(minInclusive, maxExclusive);
            _ints.Add(value);
            _batchCount++;
            return value;
        }
        else
        {
            int value = _ints[_next];
            _next++;
            _usedInCurrentBatch++;
            if (_usedInCurrentBatch == _batches[_currentBatch])
            {
                _currentBatch++;
                _usedInCurrentBatch = 0;
            }
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
            if (_currentBatch < _batches.Count && _next + _batches[_currentBatch] - 1 < _ints.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
            // return _next < _ints.Count;
        }
    }

    private void Update()
    {
        // server
        if (_batchCount > 0)
        {
            _batches.Add(_batchCount);
        }
        _batchCount = 0;
    }

    #region Singleton

    private static NetworkRandomGenerator _instance;

    public static NetworkRandomGenerator Instance { get { return _instance; } }

    #endregion
}
