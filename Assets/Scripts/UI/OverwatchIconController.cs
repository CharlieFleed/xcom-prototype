using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwatchIconController : MonoBehaviour, IUIChildController
{
    [SerializeField] OverwatchIcon _overwatchIconPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<Overwatcher, OverwatchIcon> _overwatchIcons = new Dictionary<Overwatcher, OverwatchIcon>();

    public void Init()
    {
        Overwatcher.OnOverwatcherAdded += HandleOverwatcherAdded;
        Overwatcher.OnOverwatcherRemoved += HandleOverwatcherRemoved;
    }

    private void HandleOverwatcherRemoved(Overwatcher overwatcher)
    {
        if (_overwatchIcons.ContainsKey(overwatcher) == true)
        {
            if (_overwatchIcons[overwatcher] != null)
            {
                Destroy(_overwatchIcons[overwatcher].gameObject);
            }
            _overwatchIcons.Remove(overwatcher);
        }
    }

    private void HandleOverwatcherAdded(Overwatcher overwatcher)
    {
        if (_overwatchIcons.ContainsKey(overwatcher) == false)
        {
            GridEntity gridEntity = overwatcher.GetComponent<GridEntity>();
            var overwatchIcon = Instantiate(_overwatchIconPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _overwatchIcons.Add(overwatcher, overwatchIcon);
            overwatchIcon.SetOverwatcher(overwatcher);
        }
    }

    private void OnDestroy()
    {
        Overwatcher.OnOverwatcherAdded -= HandleOverwatcherAdded;
        Overwatcher.OnOverwatcherRemoved -= HandleOverwatcherRemoved;
    }
}

