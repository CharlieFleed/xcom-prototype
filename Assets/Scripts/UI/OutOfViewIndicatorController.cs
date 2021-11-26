using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfViewIndicatorController : MonoBehaviour
{
    [SerializeField] OutOfViewIndicator _outOfViewIndicatorPrefab;

    Dictionary<GridEntity, OutOfViewIndicator> outOfViewIndicators = new Dictionary<GridEntity, OutOfViewIndicator>();

    private void OnEnable()
    {
        GridEntity.OnGridEntityAdded += HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved += HandleGridEntityRemoved;
    }

    private void OnDisable()
    {
        GridEntity.OnGridEntityAdded -= HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved -= HandleGridEntityRemoved;
    }

    private void HandleGridEntityAdded(GridEntity gridEntity)
    {
        if (outOfViewIndicators.ContainsKey(gridEntity) == false)
        {
            var outOfViewIndicator = Instantiate(_outOfViewIndicatorPrefab, transform);
            outOfViewIndicators.Add(gridEntity, outOfViewIndicator);
            outOfViewIndicator.SetGridEntity(gridEntity);
        }
    }

    private void HandleGridEntityRemoved(GridEntity gridEntity)
    {
        if (outOfViewIndicators.ContainsKey(gridEntity) == true)
        {
            if (outOfViewIndicators[gridEntity] != null)
            {
                Destroy(outOfViewIndicators[gridEntity].gameObject);
            }
            outOfViewIndicators.Remove(gridEntity);
        }
    }
}
