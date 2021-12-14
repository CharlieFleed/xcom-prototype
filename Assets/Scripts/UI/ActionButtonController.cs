using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionButtonController : MonoBehaviour
{
    [SerializeField] ActionButton _actionButtonPrefab;

    Dictionary<BattleAction, ActionButton> _actionButtons = new Dictionary<BattleAction, ActionButton>();

    private void Awake()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
    }

    void HandleActionAdded(BattleAction obj)
    {
        if (_actionButtons.ContainsKey(obj) == false)
        {
            var actionButton = Instantiate(_actionButtonPrefab, transform);
            actionButton.SetAction(obj);
            actionButton.SetShortcut(_actionButtons.Count + 1);
            _actionButtons.Add(obj, actionButton);
            int buttonWidth = (int)actionButton.GetComponent<RectTransform>().sizeDelta.x;
            GetComponent<RectTransform>().sizeDelta = new Vector2(_actionButtons.Count * buttonWidth, buttonWidth);
            actionButton.transform.localPosition = new Vector3((_actionButtons.Count - 1) * 0.5f * buttonWidth - 0.5f * buttonWidth, 0, 0);
            actionButton.OnClick += HandleActionClick;
        }
    }

    void HandleActionsCleared()
    {
        foreach (var item in _actionButtons)
        {
            Destroy(item.Value.gameObject);
        }
        _actionButtons.Clear();
    }

    private void HandleActionClick(BattleAction battleAction)
    {
        battleAction.GetComponent<UnitLocalController>().SelectAction(battleAction);
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionAdded += HandleActionAdded;
        unitLocalController.OnActionsCleared += HandleActionsCleared;
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionAdded -= HandleActionAdded;
        unitLocalController.OnActionsCleared -= HandleActionsCleared;
    }

    private void OnDestroy()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
    }
}
