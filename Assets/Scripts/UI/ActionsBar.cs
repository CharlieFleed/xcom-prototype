using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsBar : MonoBehaviour
{
    [SerializeField] List<Image> _actions;

    Character _character;

    void HandleActionsChanged(int actions)
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            _actions[i].enabled = actions > i;
        }
    }

    public void SetCharacter(Character character)
    {
        _character = character;
        _character.OnNumActionsChanged += HandleActionsChanged;
    }

    private void OnDestroy()
    {
        _character.OnNumActionsChanged -= HandleActionsChanged;
    }
}
