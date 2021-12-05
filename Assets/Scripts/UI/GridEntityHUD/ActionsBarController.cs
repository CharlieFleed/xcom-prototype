using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsBarController : MonoBehaviour, IUIChildController
{
    [SerializeField] ActionsBar _actionsBarPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<ActionsController, ActionsBar> _actionsBars = new Dictionary<ActionsController, ActionsBar>();

    public void Init()
    {
        ActionsController.OnActionsControllerAdded += HandleActionsControllerAdded;
        ActionsController.OnActionsControllerRemoved += HandleActionsControllerRemoved;
    }

    private void HandleActionsControllerRemoved(ActionsController actionsController)
    {
        if (_actionsBars.ContainsKey(actionsController) == true)
        {
            if (_actionsBars[actionsController] != null)
            {
                Destroy(_actionsBars[actionsController].gameObject);
            }
            _actionsBars.Remove(actionsController);
        }
    }

    private void HandleActionsControllerAdded(ActionsController actionsController)
    {
        if (_actionsBars.ContainsKey(actionsController) == false)
        {
            GridEntity gridEntity = actionsController.GetComponent<GridEntity>();
            var actionsBar = Instantiate(_actionsBarPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _actionsBars.Add(actionsController, actionsBar);
            actionsBar.SetActionsController(actionsController);
        }
    }

    private void OnDestroy()
    {
        ActionsController.OnActionsControllerAdded -= HandleActionsControllerAdded;
        ActionsController.OnActionsControllerRemoved -= HandleActionsControllerRemoved;
    }
}

