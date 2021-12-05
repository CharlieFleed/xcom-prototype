using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsBar : MonoBehaviour
{
    [SerializeField] List<Image> _actions;

    ActionsController _actionsController;

    void HandleActionsChanged(int actions)
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            _actions[i].enabled = actions > i;
        }
    }

    public void SetActionsController(ActionsController actionsController)
    {
        _actionsController = actionsController;
        _actionsController.OnNumActionsChanged += HandleActionsChanged;
    }

    private void OnDestroy()
    {
        _actionsController.OnNumActionsChanged -= HandleActionsChanged;
    }
}
