using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityHUD : MonoBehaviour
{
    [SerializeField] float _transparency = 0.5f;
    [SerializeField] int _positionOffset = 3;

    Unit _unit;
    Health _health;
    GridEntity _gridEntity;
    Viewer _viewer;

    Camera _camera;

    CanvasGroup _cg;
    int _display; // 0: hide, 1: transparent, 2: opaque 
    float _fadeSpeed = 15f;

    private void Awake()
    {
        _camera = Camera.main;
        _cg = GetComponent<CanvasGroup>();
        _display = 0;
    }

    public void SetGridEntity(GridEntity gridEntity)
    {
        _gridEntity = gridEntity;
        _unit = _gridEntity.GetComponent<Unit>();
        _health = _gridEntity.GetComponent<Health>();
        _viewer = _gridEntity.GetComponent<Viewer>();
    }

    private void Update()
    {
        Check();
        Fade();
    }

    private void Check()
    {
        if (_health.IsDead || (_viewer != null && !_viewer.IsVisible))
        {
            _display = 0;
        }
        else if ((_unit != null && _unit.IsActive) ||
            _gridEntity.IsTargeted ||
            _gridEntity.IsSoftTargeted ||
            _health.IsDamaged ||
            (_unit != null && (NetworkMatchManager.Instance.CurrentUnit != null && NetworkMatchManager.Instance.CurrentUnit.GetComponent<Walker>().IsActive && !NetworkMatchManager.Instance.CurrentUnit.GetComponent<Walker>().IsWalking && !_unit.Team.IsActive)))
        {
            _display = 2;
        }
        else
        {
            _display = 1;
        }
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

    private void LateUpdate()
    {
        transform.position = _camera.WorldToScreenPoint(_gridEntity.transform.position + Vector3.up * _positionOffset);
    }
}
