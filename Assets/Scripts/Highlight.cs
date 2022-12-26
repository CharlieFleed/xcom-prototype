using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    public bool Highlighted;
    public bool Silhouetted;
    [SerializeField] Material _highlightMaterial;
    [SerializeField] Material _silhouetteMaterial;
    Outline[] _outlines;

    Renderer[] _renderers;
    Material[] _originalMaterials;

    private void Awake()
    {
        _outlines = GetComponentsInChildren<Outline>();
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
            for (int i = 0; i < _outlines.Length; i++)
            {
                _outlines[i].enabled = false;
            }
            foreach (var renderer in _renderers)
            {
                //renderer.materials[0] = _highlightMaterial;
                //renderer.materials = new Material[] { _highlightMaterial };
                renderer.material = _highlightMaterial;
            }
            Highlighted = false;
        }
        else if (Silhouetted)
        {
            //Debug.Log($"{name} is silhouetted.");
            for (int i = 0; i < _outlines.Length; i++)
            {
                _outlines[i].enabled = false;
            }
            foreach (var renderer in _renderers)
            {
                //renderer.materials[0] = _silhouetteMaterial;
                //renderer.materials = new Material[] { _silhouetteMaterial };
                renderer.material = _silhouetteMaterial;
            }
            Silhouetted = false;
        }
        else
        {
            for (int i = 0; i < _outlines.Length; i++)
            {
                _outlines[i].enabled = true;
            }
            for (int i = 0; i < _renderers.Length; i++)
            {
                //_renderers[i].materials[0] = _originalMaterials[i];
                //_renderers[i].materials = new Material[] { _originalMaterials[i] };
                _renderers[i].material = _originalMaterials[i];
            }
        }
    }
}
