using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void Awake()
    {
        Character.OnCharacterAdded += Character_OnCharacterAdded;
        Character.OnCharacterRemoved += Character_OnCharacterRemoved;
    }

    private void Character_OnCharacterAdded(Character obj)
    {
        OnClick += obj.HandleEndTurn;
    }

    private void Character_OnCharacterRemoved(Character obj)
    {
        OnClick -= obj.HandleEndTurn;
    }

    private void OnDestroy()
    {
        Character.OnCharacterAdded -= Character_OnCharacterAdded;
        Character.OnCharacterRemoved -= Character_OnCharacterRemoved;
    }

    public void Click()
    {
        OnClick();
    }
}
