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
        Character.OnCharacterAdded += HandleCharacterAdded;
        Character.OnCharacterRemoved += HandleCharacterRemoved;
    }

    void HandleActionAdded(BattleAction obj)
    {
        if (_actionButtons.ContainsKey(obj) == false)
        {
            var actionButton = Instantiate(_actionButtonPrefab, transform);
            actionButton.SetAction(obj);
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
        battleAction.GetComponent<Character>().SelectAction(battleAction);
    }

    private void HandleCharacterAdded(Character character)
    {
        character.OnActionAdded += HandleActionAdded;
        character.OnActionsCleared += HandleActionsCleared;
    }

    private void HandleCharacterRemoved(Character character)
    {
        character.OnActionAdded -= HandleActionAdded;
        character.OnActionsCleared -= HandleActionsCleared;
    }

    private void OnDestroy()
    {
        Character.OnCharacterAdded -= HandleCharacterAdded;
        Character.OnCharacterRemoved -= HandleCharacterRemoved;
    }
}
