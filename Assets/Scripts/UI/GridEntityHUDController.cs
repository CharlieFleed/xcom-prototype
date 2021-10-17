using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityHUDController : MonoBehaviour
{
    [SerializeField] GridEntityHUD _characterHUDPrefab;

    Dictionary<GridEntity, GridEntityHUD> _characterHUDs = new Dictionary<GridEntity, GridEntityHUD>();

    IUIChildController[] _children;

    private void Awake()
    {
        //Debug.Log("GridEntityHUDController Awake.");
        GridEntity.OnGridEntityAdded += HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved += HandleGridEntityRemoved;
        _children = GetComponents<IUIChildController>();
        for (int i = 0; i < _children.Length; i++)
        {
            _children[i].Init();
        }
    }

    private void HandleGridEntityAdded(GridEntity gridEntity)
    {
        //Debug.Log("GridEntityHUDController HandleGridEntityAdded.");
        if (_characterHUDs.ContainsKey(gridEntity) == false)
        {
            var gridEntityHUD = Instantiate(_characterHUDPrefab, transform);
            _characterHUDs.Add(gridEntity, gridEntityHUD);
            gridEntityHUD.SetGridEntity(gridEntity);
        }
    }

    private void HandleGridEntityRemoved(GridEntity gridEntity)
    {
        if (_characterHUDs.ContainsKey(gridEntity) == true)
        {
            if (_characterHUDs[gridEntity] != null)
            {
                Destroy(_characterHUDs[gridEntity].gameObject);
            }
            _characterHUDs.Remove(gridEntity);
        }
    }

    public GridEntityHUD GridEntityHUD(GridEntity gridEntity)
    {
        if (_characterHUDs.ContainsKey(gridEntity))
        {
            return _characterHUDs[gridEntity];
        }
        else
        {
            return null;
        }
    }

    private void OnDestroy()
    {
        GridEntity.OnGridEntityAdded -= HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved -= HandleGridEntityRemoved;
    }
}
