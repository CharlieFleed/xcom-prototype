using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeThroughGradient : MonoBehaviour
{
    bool _shouldHide = true;
    float _minAlpha = 0.1f;
    float _maxAlpha = 1.0f;
    float _currentAlpha = 1.0f;
    float _fadeSpeed = 1f;
    Color _baseColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    public bool Hidden
    {
        set
        {
            _hidden = value;
            if (value)
            {
                if (_group != null)
                {
                    _group.Hide();
                }
            }
        }
        get { return _hidden; }
    }
    bool _hidden;
    bool _prevHidden;

    [SerializeField] Material[] _materials;

    Renderer[] _renderers;
    Material[] _originalMaterials;
    Color[] _originalColors;

    SeeThroughGroup _group;
    GridObject _gridObject;

    CameraController _cameraController;

    private void Awake()
    {
        Transform parent = transform.parent;
        if (parent != null)
        {
            _group = parent.GetComponent<SeeThroughGroup>();
        }
    }

    private void OnEnable()
    {
        CameraDirector.OnAimingCameraActiveChanged += CameraDirector_OnAimingCameraActiveChanged;
    }

    private void OnDisable()
    {
        CameraDirector.OnAimingCameraActiveChanged -= CameraDirector_OnAimingCameraActiveChanged;
    }

    private void CameraDirector_OnAimingCameraActiveChanged(bool active)
    {
        _shouldHide = !active;
    }

    // Start is called before the first frame update
    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalMaterials = new Material[_renderers.Length];
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].material;
        }
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i].material.HasProperty("_BaseColor"))
            {
                _originalColors[i] = _renderers[i].material.GetColor("_BaseColor");
            }
            else if (_renderers[i].material.HasProperty("_Color"))
            {
                _originalColors[i] = _renderers[i].material.color;
            }
        }
        _gridObject = GetComponent<GridObject>();
        _cameraController = CameraController.Instance;
    }

    private void LateUpdate()
    {
        if (Hidden && _shouldHide)
        {
            foreach (var gridNode in _gridObject.floorNodes)
            {
                if (gridNode.Y > _cameraController.Level && gridNode.IsOccupied())
                {
                    Highlight occupier;
                    if (gridNode.GetOccupierOfType<Highlight>(out occupier))
                    {
                        occupier.Silhouetted = true;
                    }
                }
            }
        }
        // fade in/out
        if (Hidden && _shouldHide)
        {
            _currentAlpha = Mathf.Lerp(_currentAlpha, _minAlpha, _fadeSpeed * Time.deltaTime);
            Debug.Log($"alpha {_currentAlpha}");
        }
        else
        {
            _currentAlpha = 1.0f;
        }
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
            {
                if (_renderers[i].material.HasProperty("_BaseColor"))
                {
                    _renderers[i].material.SetColor("_BaseColor", new Color(_originalColors[i].r, _originalColors[i].g, _originalColors[i].b, _currentAlpha));
                }
                else if (_renderers[i].material.HasProperty("_Color"))
                {
                    _renderers[i].material.color = new Color(_originalColors[i].r, _originalColors[i].g, _originalColors[i].b, _currentAlpha);
                }
            }
        }

        _prevHidden = _hidden;
        _hidden = false;
    }

    public void HideBrother()
    {
        //Debug.Log($"{name} hidden by brother.");
        _hidden = true;
    }
}
