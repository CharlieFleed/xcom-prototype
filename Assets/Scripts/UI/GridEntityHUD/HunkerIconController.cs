using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunkerIconController : MonoBehaviour, IUIChildController
{
    [SerializeField] HunkerIcon _hunkerIconPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<Hunkerer, HunkerIcon> _hunkerIcons = new Dictionary<Hunkerer, HunkerIcon>();

    public void Init()
    {
        Hunkerer.OnHunkererAdded += HandleHunkererAdded;
        Hunkerer.OnHunkererRemoved += HandleHunkererRemoved;
    }

    private void HandleHunkererRemoved(Hunkerer hunkerer)
    {
        if (_hunkerIcons.ContainsKey(hunkerer) == true)
        {
            if (_hunkerIcons[hunkerer] != null)
            {
                Destroy(_hunkerIcons[hunkerer].gameObject);
            }
            _hunkerIcons.Remove(hunkerer);
        }
    }

    private void HandleHunkererAdded(Hunkerer hunkerer)
    {
        if (_hunkerIcons.ContainsKey(hunkerer) == false)
        {
            GridEntity gridEntity = hunkerer.GetComponent<GridEntity>();
            var hunkerIcon = Instantiate(_hunkerIconPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _hunkerIcons.Add(hunkerer, hunkerIcon);
            hunkerIcon.SetHunkerer(hunkerer);
        }
    }

    private void OnDestroy()
    {
        Hunkerer.OnHunkererAdded -= HandleHunkererAdded;
        Hunkerer.OnHunkererRemoved -= HandleHunkererRemoved;
    }
}

