using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsBarController : MonoBehaviour, IUIChildController
{
    [SerializeField] ActionsBar _actionsBarPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<Unit, ActionsBar> _actionsBars = new Dictionary<Unit, ActionsBar>();

    public void Init()
    {
        Unit.OnUnitAdded += HandleUnitAdded;
        Unit.OnUnitRemoved += HandleUnitRemoved;
    }

    private void HandleUnitRemoved(Unit unit)
    {
        if (_actionsBars.ContainsKey(unit) == true)
        {
            if (_actionsBars[unit] != null)
            {
                Destroy(_actionsBars[unit].gameObject);
            }
            _actionsBars.Remove(unit);
        }
    }

    private void HandleUnitAdded(Unit unit)
    {
        if (_actionsBars.ContainsKey(unit) == false)
        {
            GridEntity gridEntity = unit.GetComponent<GridEntity>();
            var actionsBar = Instantiate(_actionsBarPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _actionsBars.Add(unit, actionsBar);
            actionsBar.SetUnit(unit);
        }
    }

    private void OnDestroy()
    {
        Unit.OnUnitAdded -= HandleUnitAdded;
        Unit.OnUnitRemoved -= HandleUnitRemoved;
    }
}

