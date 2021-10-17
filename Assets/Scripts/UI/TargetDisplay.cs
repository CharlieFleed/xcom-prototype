using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDisplay : MonoBehaviour
{
    [SerializeField] GridEntity _gridEntity;
    [SerializeField] GameObject _target;

    CanvasGroup _cg;
    Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _cg = _target.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        _cg.alpha = (_gridEntity.IsTargeted || _gridEntity.IsSoftTargeted) ? 1 : 0;
    }

    private void LateUpdate()
    {
        _target.transform.position = _gridEntity.transform.position - _camera.transform.forward;
    }
}
