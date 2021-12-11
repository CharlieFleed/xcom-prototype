using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmActionButton : MonoBehaviour
{
    [SerializeField] Button _button;

    public event Action OnClick;
    
    private void Awake()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
        gameObject.SetActive(false);
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionActivated += HandleActionActivated;
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionActivated -= HandleActionActivated;
    }

    private void HandleActionActivated(BattleAction battleAction)
    {
        if (battleAction.IsConfirmable)
        {
            OnClick += battleAction.HandleConfirmClick;
            battleAction.OnActionConfirmed += HandleActionConfirmed;
            battleAction.OnActionCancelled += HandleActionCancelled;
            GetComponentInChildren<Text>().text = battleAction.ConfirmText;
            gameObject.SetActive(true);
        }
    }

    public void Click()
    {
        OnClick();
    }

    void HandleActionConfirmed(BattleAction battleAction)
    {
        OnClick -= battleAction.HandleConfirmClick;
        battleAction.OnActionConfirmed -= HandleActionConfirmed;
        battleAction.OnActionCancelled -= HandleActionCancelled;
        gameObject.SetActive(false);
    }

    void HandleActionCancelled(BattleAction battleAction)
    {
        OnClick -= battleAction.HandleConfirmClick;
        battleAction.OnActionConfirmed -= HandleActionConfirmed;
        battleAction.OnActionCancelled -= HandleActionCancelled;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
    }
}
