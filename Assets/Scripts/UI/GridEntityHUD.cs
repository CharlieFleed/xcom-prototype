using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntityHUD : MonoBehaviour
{
    [SerializeField] float _transparency = 0.5f;
    [SerializeField] int _positionOffset = 2;

    Character _character;
    Health _health;
    GridEntity _gridEntity;

    Camera _camera;

    CanvasGroup _cg;
    int _display; // 0: hide, 1: transparent, 2: opaque 
    float _fadeSpeed = 15f;

    private void Awake()
    {
        _camera = Camera.main;
        _cg = GetComponent<CanvasGroup>();
    }

    public void SetGridEntity(GridEntity gridEntity)
    {
        _gridEntity = gridEntity;
        _character = _gridEntity.GetComponent<Character>();
        _health = _gridEntity.GetComponent<Health>();
    }

    private void Update()
    {
        if (_health.IsDead)
        {
            _display = 0;
        }
        else if ((_character != null && _character.IsActive) ||
            _gridEntity.IsTargeted ||
            _gridEntity.IsSoftTargeted ||
            _health.IsDamaged ||
            (_character != null && (MatchManager.Instance.ActiveCharacter != null && MatchManager.Instance.ActiveCharacter.GetComponent<Walker>().IsActive && !MatchManager.Instance.ActiveCharacter.GetComponent<Walker>().IsWalking && !_character.Team.IsActive)))
        {
            _display = 2;
        }
        else
        {
            _display = 1;
        }
        Fade();
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
