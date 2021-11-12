using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextUnitButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void Awake()
    {
        Unit.OnUnitAdded += HandleUnit_OnUnitAdded;
        Unit.OnUnitRemoved += HandleUnit_OnUnitRemoved;
    }

    private void HandleUnit_OnUnitAdded(Unit obj)
    {
        OnClick += obj.HandlePass;
    }

    private void HandleUnit_OnUnitRemoved(Unit obj)
    {
        OnClick -= obj.HandlePass;
    }

    private void OnDestroy()
    {
        Unit.OnUnitAdded -= HandleUnit_OnUnitAdded;
    }

    public void Click()
    {
        OnClick();
    }
}
