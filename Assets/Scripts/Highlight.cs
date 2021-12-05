using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    public bool Highlighted;
    public bool Silhouetted;
    [SerializeField] Material _highlightMaterial;
    [SerializeField] Material _silhouetteMaterial;
    Outline _outline;

    Renderer[] _renderers;
    Material[] _originalMaterials;

    private void Awake()
    {
        _outline = GetComponentInChildren<Outline>();
    }

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
            //Debug.Log($"{name} is highlighted.");
            if (_outline) _outline.enabled = false;
            foreach (var renderer in _renderers)
            {
                renderer.material = _highlightMaterial;
            }
            Highlighted = false;
        }
        else if (Silhouetted)
        {
            //Debug.Log($"{name} is silhouetted.");
            if (_outline) _outline.enabled = false;
            foreach (var renderer in _renderers)
            {
                renderer.material = _silhouetteMaterial;
            }
            Silhouetted = false;
        }
        else
        {
            if (_outline) _outline.enabled = true;
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material = _originalMaterials[i];
            }
        }
    }
}
