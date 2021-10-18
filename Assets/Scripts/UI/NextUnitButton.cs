using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextUnitButton : MonoBehaviour
{
    public event Action OnClick = delegate { };

    private void Awake()
    {
        Character.OnCharacterAdded += Character_OnCharacterAdded;
        Character.OnCharacterRemoved += Character_OnCharacterRemoved;
    }

    private void Character_OnCharacterAdded(Character obj)
    {
        OnClick += obj.HandlePass;
    }

    private void Character_OnCharacterRemoved(Character obj)
    {
        OnClick -= obj.HandlePass;
    }

    private void OnDestroy()
    {
        Character.OnCharacterAdded -= Character_OnCharacterAdded;
    }

    public void Click()
    {
        OnClick();
    }
}
