using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPivot : MonoBehaviour
{
    [SerializeField] GameObject _pivot;
    [SerializeField] float _positionOffset;
    Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        transform.position = _camera.WorldToScreenPoint(_pivot.transform.position + Vector3.up * _positionOffset);
    }
}
