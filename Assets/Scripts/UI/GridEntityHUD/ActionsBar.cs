using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsBar : MonoBehaviour
{
    [SerializeField] List<Image> _actions;

    Unit _unit;

    void HandleActionsChanged(int actions)
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            _actions[i].enabled = actions > i;
        }
    }

    public void SetUnit(Unit unit)
    {
        _unit = unit;
        _unit.OnNumActionsChanged += HandleActionsChanged;
    }

    private void OnDestroy()
    {
        _unit.OnNumActionsChanged -= HandleActionsChanged;
    }
}
