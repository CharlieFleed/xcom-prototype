using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityHUDController : MonoBehaviour
{
    [SerializeField] GridEntityHUD _gridEntityHUDPrefab;

    Dictionary<GridEntity, GridEntityHUD> _gridEntityHUDs = new Dictionary<GridEntity, GridEntityHUD>();

    IUIChildController[] _children;

    private void Awake()
    {
        //Debug.Log("GridEntityHUDController Awake.");
        _children = GetComponents<IUIChildController>();
        for (int i = 0; i < _children.Length; i++)
        {
            _children[i].Init();
        }
    }

    private void OnEnable()
    {
        GridEntity.OnGridEntityAdded += HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved += HandleGridEntityRemoved;
    }

    private void OnDisable()
    {
        GridEntity.OnGridEntityAdded -= HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved -= HandleGridEntityRemoved;
    }

    private void HandleGridEntityAdded(GridEntity gridEntity)
    {
        //Debug.Log("GridEntityHUDController HandleGridEntityAdded.");
        if (_gridEntityHUDs.ContainsKey(gridEntity) == false)
        {
            var gridEntityHUD = Instantiate(_gridEntityHUDPrefab, transform);
            _gridEntityHUDs.Add(gridEntity, gridEntityHUD);
            gridEntityHUD.SetGridEntity(gridEntity);
        }
    }

    private void HandleGridEntityRemoved(GridEntity gridEntity)
    {
        if (_gridEntityHUDs.ContainsKey(gridEntity) == true)
        {
            if (_gridEntityHUDs[gridEntity] != null)
            {
                Destroy(_gridEntityHUDs[gridEntity].gameObject);
            }
            _gridEntityHUDs.Remove(gridEntity);
        }
    }

    public GridEntityHUD GridEntityHUD(GridEntity gridEntity)
    {
        if (_gridEntityHUDs.ContainsKey(gridEntity))
        {
            return _gridEntityHUDs[gridEntity];
        }
        else
        {
            return null;
        }
    }
}
