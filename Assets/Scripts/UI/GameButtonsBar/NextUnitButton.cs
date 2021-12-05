using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextUnitButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void Awake()
    {
        UnitLocalController.OnUnitAdded += HandleUnit_OnUnitAdded;
        UnitLocalController.OnUnitRemoved += HandleUnit_OnUnitRemoved;
    }

    private void HandleUnit_OnUnitAdded(UnitLocalController obj)
    {
        OnClick += obj.HandlePass;
    }

    private void HandleUnit_OnUnitRemoved(UnitLocalController obj)
    {
        OnClick -= obj.HandlePass;
    }

    private void OnDestroy()
    {
        UnitLocalController.OnUnitAdded -= HandleUnit_OnUnitAdded;
    }

    public void Click()
    {
        OnClick();
    }
}
