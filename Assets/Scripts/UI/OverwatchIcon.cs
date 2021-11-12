using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverwatchIcon : MonoBehaviour
{
    [SerializeField] Image _icon;

    Overwatcher _overwatcher;

    public void SetOverwatcher(Overwatcher overwatcher)
    {
        _overwatcher = overwatcher;
    }

    private void Update()
    {
        _icon.enabled = _overwatcher.IsOverwatching;
    }
}
