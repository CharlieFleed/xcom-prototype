using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void OnEnable()
    {
        Unit.OnUnitAdded += HandleUnit_OnUnitAdded;
        Unit.OnUnitRemoved += HandleUnit_OnUnitRemoved;
    }

    private void OnDisable()
    {
        Unit.OnUnitAdded -= HandleUnit_OnUnitAdded;
        Unit.OnUnitRemoved -= HandleUnit_OnUnitRemoved;
    }

    private void HandleUnit_OnUnitAdded(Unit unit)
    {
        OnClick += unit.HandleEndTurn;
    }

    private void HandleUnit_OnUnitRemoved(Unit unit)
    {
        OnClick -= unit.HandleEndTurn;
    }

    public void Click()
    {
        OnClick();
    }
}
