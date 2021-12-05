using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void OnEnable()
    {
        UnitLocalController.OnUnitAdded += HandleUnit_OnUnitAdded;
        UnitLocalController.OnUnitRemoved += HandleUnit_OnUnitRemoved;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitAdded -= HandleUnit_OnUnitAdded;
        UnitLocalController.OnUnitRemoved -= HandleUnit_OnUnitRemoved;
    }

    private void HandleUnit_OnUnitAdded(UnitLocalController unit)
    {
        OnClick += unit.HandleEndTurn;
    }

    private void HandleUnit_OnUnitRemoved(UnitLocalController unit)
    {
        OnClick -= unit.HandleEndTurn;
    }

    public void Click()
    {
        OnClick();
    }
}
