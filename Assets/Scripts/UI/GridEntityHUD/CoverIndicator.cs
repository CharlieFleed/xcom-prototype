using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoverIndicator : MonoBehaviour
{
    [SerializeField] Image _cover;
    [SerializeField] Image _halfCover;
    [SerializeField] Image _flanked;

    GridAgent _gridAgent;

    public void SetGridAgent(GridAgent gridAgent)
    {
        _gridAgent = gridAgent;
        _gridAgent.OnCoverChanged += HandleCoverChanged;
    }

    void HandleCoverChanged(int cover)
    {
        _flanked.enabled = cover == -1;
        _halfCover.enabled = cover == 1;
        _cover.enabled = cover == 2;
    }

    private void OnDestroy()
    {
        _gridAgent.OnCoverChanged -= HandleCoverChanged;
    }
}
