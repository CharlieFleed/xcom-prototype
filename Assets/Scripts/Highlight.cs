using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    public bool Highlighted;
    public bool Silhouetted;
    [SerializeField] Material _highlightMaterial;
    [SerializeField] Material _silhouetteMaterial;

    Renderer[] _renderers;
    Material[] _originalMaterials;

    // Start is called before the first frame update
    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].material;
        }
    }

    private void LateUpdate()
    {
        if (Highlighted)
        {
            foreach (var renderer in _renderers)
            {
                renderer.material = _highlightMaterial;
            }
            Highlighted = false;
        }
        else if (Silhouetted)
        {
            foreach (var renderer in _renderers)
            {
                renderer.material = _silhouetteMaterial;
            }
            Silhouetted = false;
        }
        else
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material = _originalMaterials[i];
            }
        }
    }
}
