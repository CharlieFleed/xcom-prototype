using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeThroughBackup : MonoBehaviour
{
    bool _shouldHide = true;

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
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].material;
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
        if (Hidden != _prevHidden)
        {
            if (Hidden && _shouldHide)
            {
                //Debug.Log($"{name} hidden.");
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                    {
                        //_renderers[i].enabled = false;
                        _renderers[i].material = _materials[0];
                        _renderers[i].materials = _materials;
                    }
                }

            }
            else
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i])
                    {
                        //_renderers[i].enabled = true;
                        _renderers[i].material = _originalMaterials[i];
                        _renderers[i].materials = new Material[] { _originalMaterials[i] };
                    }
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
