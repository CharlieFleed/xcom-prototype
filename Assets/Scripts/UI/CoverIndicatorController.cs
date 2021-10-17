using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverIndicatorController : MonoBehaviour, IUIChildController
{
    [SerializeField] CoverIndicator _coverIndicatorPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<GridAgent, CoverIndicator> _coverIndicators = new Dictionary<GridAgent, CoverIndicator>();

    public void Init()
    {
        GridAgent.OnGridAgentAdded += HandleGridAgentAdded;
        GridAgent.OnGridAgentRemoved += HandleGridAgentRemoved;
    }

    private void HandleGridAgentRemoved(GridAgent gridAgent)
    {
        if (_coverIndicators.ContainsKey(gridAgent) == true)
        {
            if (_coverIndicators[gridAgent] != null)
            {
                Destroy(_coverIndicators[gridAgent].gameObject);
            }
            _coverIndicators.Remove(gridAgent);
        }
    }

    private void HandleGridAgentAdded(GridAgent gridAgent)
    {
        if (_coverIndicators.ContainsKey(gridAgent) == false)
        {
            GridEntity gridEntity = gridAgent.GetComponent<GridEntity>();
            var coverIndicator = Instantiate(_coverIndicatorPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _coverIndicators.Add(gridAgent, coverIndicator);
            coverIndicator.SetGridAgent(gridAgent);
        }
    }

    private void OnDestroy()
    {
        GridAgent.OnGridAgentAdded -= HandleGridAgentAdded;
        GridAgent.OnGridAgentRemoved -= HandleGridAgentRemoved;
    }
}
