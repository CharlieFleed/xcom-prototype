using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutOfViewIndicator : MonoBehaviour
{
    [SerializeField] Image _indicator;
    [SerializeField] float _transparency = 0.5f;
 
    Camera _camera;
    GridEntity _gridEntity;
    Viewer _viewer;

    int _display; // 0: hide, 1: transparent, 2: opaque 
    CanvasGroup _cg;
    float _fadeSpeed = 15f;

    Vector3 _viewportPoint;

    bool _initialized = false;

    private void Awake()
    {
        _camera = Camera.main;
        _cg = GetComponent<CanvasGroup>();
    }

    public void SetGridEntity(GridEntity gridEntity)
    {
        _gridEntity = gridEntity;
        _viewer = _gridEntity.GetComponent<Viewer>();
    }

    private void Update()
    {
        if (!_initialized)
        {
            Initialize();
        }
        Fade();
    }

    private void Initialize()
    {
        Unit unit = _gridEntity.GetComponent<Unit>();
        if (unit != null)
        {
            if (unit.Team != null)
            {
                _indicator.color = Team.TeamColors[int.Parse(unit.Team.Name)];
                _initialized = true;
            }
            else
            {
                Debug.Log($"No team for health bar of {unit.name}");
            }
        }
        else
        {
            _indicator.color = Color.white;
            _initialized = true;
        }
    }

    private void LateUpdate()
    {
        if (_viewer != null && !_viewer.IsVisible)
        {
            //Debug.Log("invisible");
            _display = 0;
            return;
        }
        Vector3 viewPos = _camera.WorldToViewportPoint(_gridEntity.transform.position);
        if ((viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1) || viewPos.z < 0)
        {
            _display = 0;
        }
        else
        {
            _display = 2;
            if (viewPos.x == .5f)
            {
                _viewportPoint.x = .5f;
                if (viewPos.y > 1)
                {
                    _viewportPoint.y = .98f;
                }
                else
                {
                    _viewportPoint.y = .02f;
                }
            }
            else if (viewPos.y == .5f)
            {
                _viewportPoint.y = .5f;
                if (viewPos.x > 1)
                {
                    _viewportPoint.x = .98f;
                }
                else
                {
                    _viewportPoint.x = .02f;
                }
            }
            else
            {
                if ((viewPos.x > 1) && (viewPos.y >= 1 - viewPos.x) && (viewPos.y <= viewPos.x)) // yellow
                {
                    //Debug.Log("yellow");
                    _viewportPoint.x = .98f;
                    _viewportPoint.y = (viewPos.y - .5f) * (_viewportPoint.x - .5f) / (viewPos.x - .5f) + .5f;
                }
                else if ((viewPos.x < 0) && (viewPos.y <= 1 - viewPos.x) && (viewPos.y >= viewPos.x)) // green
                {
                    //Debug.Log("green");
                    _viewportPoint.x = .02f;
                    _viewportPoint.y = (viewPos.y - .5f) * (_viewportPoint.x - .5f) / (viewPos.x - .5f) + .5f;
                }
                else if ((viewPos.y > 1) && (viewPos.y > 1 - viewPos.x) && (viewPos.y > viewPos.x)) // light blue
                {
                    //Debug.Log("light blue");
                    _viewportPoint.y = .98f;
                    _viewportPoint.x = (viewPos.x - .5f) * (_viewportPoint.y - .5f) / (viewPos.y - .5f) + .5f;
                }
                else if ((viewPos.y < 0) && (viewPos.y < 1 - viewPos.x) && (viewPos.y < viewPos.x)) // purple
                {
                    //Debug.Log("purple");
                    _viewportPoint.y = .02f;
                    _viewportPoint.x = (viewPos.x - .5f) * (_viewportPoint.y - .5f) / (viewPos.y - .5f) + .5f;
                }
            }
        }
        _viewportPoint.z = viewPos.z;
        transform.position = _camera.ViewportToScreenPoint(_viewportPoint);
    }

    private void Fade()
    {
        if (_display == 0)
        {
            _cg.alpha = Mathf.Lerp(_cg.alpha, 0, _fadeSpeed * Time.deltaTime);
        }
        else if (_display == 1)
        {
            _cg.alpha = Mathf.Lerp(_cg.alpha, _transparency, _fadeSpeed * Time.deltaTime);
        }
        else
        {
            _cg.alpha = Mathf.Lerp(_cg.alpha, 1, _fadeSpeed * Time.deltaTime);
        }
    }
}
