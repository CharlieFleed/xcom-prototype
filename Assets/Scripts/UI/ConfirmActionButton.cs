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
        Character.OnCharacterAdded += HandleCharacterAdded;
        Character.OnCharacterRemoved += HandleCharacterRemoved;
        gameObject.SetActive(false);
    }

    private void HandleCharacterAdded(Character character)
    {
        character.OnActionActivated += HandleActionActivated;
    }

    private void HandleCharacterRemoved(Character character)
    {
        character.OnActionActivated -= HandleActionActivated;
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
        Character.OnCharacterAdded -= HandleCharacterAdded;
        Character.OnCharacterRemoved -= HandleCharacterRemoved;
    }
}
