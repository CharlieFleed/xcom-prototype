using UnityEngine;
using System.Collections;
using System;

public class GridEntity : MonoBehaviour
{
    public Sprite Icon;

    public static event Action<GridEntity> OnGridEntityAdded = delegate { };
    public static event Action<GridEntity> OnGridEntityRemoved = delegate { };

    private bool _isTargeted;
    public bool IsTargeted { get { return _isTargeted; } set { _isTargeted = value; OnTargetedChanged(_isTargeted); } }
    public event Action<bool> OnTargetedChanged = delegate { };

    private bool _isSoftTargeted;
    public bool IsSoftTargeted { get { return _isSoftTargeted; } set { _isSoftTargeted = value; OnSoftTargetedChanged(_isSoftTargeted); } }
    public event Action<bool> OnSoftTargetedChanged = delegate { };

    public GridNode CurrentNode { get; set; }

    private void OnEnable()
    {
        OnGridEntityAdded(this); // NOTE: this needs to happen before OnUnitAdded, OnHealthAdded etc 
    }

    private void Start()
    {
        //Debug.Log($"GridEntity Start for {name}.");
        CurrentNode = GridManager.Instance.GetGridNodeFromWorldPosition(transform.position);
    }

    private void OnDestroy()
    {
        OnGridEntityRemoved(this);
    }
}
