using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HunkerIcon : MonoBehaviour
{
    [SerializeField] Image _icon;

    Hunkerer _hunkerer;

    public void SetHunkerer(Hunkerer hunkerer)
    {
        _hunkerer = hunkerer;
    }

    private void Update()
    {
        _icon.enabled = _hunkerer.IsHunkering;
    }
}
