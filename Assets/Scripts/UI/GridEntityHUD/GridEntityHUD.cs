using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityHUD : MonoBehaviour
{
    [SerializeField] float _transparency = 0.5f;
    [SerializeField] int _positionOffset = 3;

    UnitLocalController _unit;
    Health _health;
    GridEntity _gridEntity;
    Viewer _viewer;
    TeamMember _teamMember;

    Camera _camera;

    CanvasGroup _cg;
    int _display; // 0: hide, 1: transparent, 2: opaque 
    float _fadeSpeed = 15f;
    float _displayTimer = 0; // used to force display

    Vector3 _screenPoint;

    private void Awake()
    {
        _camera = Camera.main;
        _cg = GetComponent<CanvasGroup>();
        _display = 0;
    }

    public void SetGridEntity(GridEntity gridEntity)
    {
        _gridEntity = gridEntity;
        _unit = _gridEntity.GetComponent<UnitLocalController>();
        _health = _gridEntity.GetComponent<Health>();
        _health.OnTakeDamage += HandleHealth_TakeDamage;
        _viewer = _gridEntity.GetComponent<Viewer>();
        _teamMember = _gridEntity.GetComponent<TeamMember>();
    }

    private void OnDestroy()
    {
        _health.OnTakeDamage -= HandleHealth_TakeDamage;
    }

    private void Update()
    {
        Check();
        Fade();
    }

    private void Check()
    {
        if (_displayTimer > 0)
        {
            _displayTimer -= Time.deltaTime;
            _display = 2;
            return;
        }
        if (_health.IsDead || (_viewer != null && !_viewer.IsVisible))
        {
            _display = 0;
        }
        else if (_screenPoint.z < 0)
        {
            _display = 0;
        }
        else if ((_unit != null && _unit.IsActive) ||
            _gridEntity.IsTargeted ||
            _gridEntity.IsSoftTargeted ||
            (_unit != null && (NetworkMatchManager.Instance.CurrentTeamMember != null && NetworkMatchManager.Instance.CurrentTeamMember.GetComponent<Walker>().IsActive && !NetworkMatchManager.Instance.CurrentTeamMember.GetComponent<Walker>().IsWalking && !_teamMember.Team.IsActive)))
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
        _screenPoint = _camera.WorldToScreenPoint(_gridEntity.transform.position);
        transform.position = _camera.WorldToScreenPoint(_gridEntity.transform.position + Vector3.up * _positionOffset);
    }

    void HandleHealth_TakeDamage(Health health, int damage, bool hit, bool crit)
    {
        _displayTimer = 2;
    }
}
