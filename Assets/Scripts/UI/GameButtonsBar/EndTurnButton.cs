using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void OnEnable()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController unitLocalController)
    {
        OnClick += unitLocalController.HandleEndTurn;
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController unitLocalController)
    {
        OnClick -= unitLocalController.HandleEndTurn;
    }

    public void Click()
    {
        OnClick();
    }
}
