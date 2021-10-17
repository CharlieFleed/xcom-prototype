using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNodeHighlight : MonoBehaviour
{
    public bool Highlighted = false;
    bool _prevHighlighted = false;
    Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (Highlighted != _prevHighlighted)
        {
            if (Highlighted)
            {
                _renderer.enabled = true;
            }
            else
            {
                _renderer.enabled = false;
            }
        }
        _prevHighlighted = Highlighted;
        Highlighted = false;
    }
}
