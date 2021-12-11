using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextUnitButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void Awake()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController obj)
    {
        OnClick += obj.HandlePass;
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController obj)
    {
        OnClick -= obj.HandlePass;
    }

    private void OnDestroy()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
    }

    public void Click()
    {
        OnClick();
    }
}
