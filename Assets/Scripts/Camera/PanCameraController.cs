using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCameraController : MonoBehaviour
{
    [SerializeField] CameraDirector _cameraDirector;
    [SerializeField] CinemachineVirtualCamera _camera;

    [SerializeField] bool _useScreenBorders = false;

    float _panSpeed = 20f;
    float _panBorderThickness = 10f;

    GameObject _followTarget;

    Vector3 _offset = new Vector3();
    Transform _savedCameraFollow;
    Vector3 _savedCameraFollowPosition;

    private void Awake()
    {
        _followTarget = new GameObject("PanCameraFollow");
        _followTarget.transform.SetParent(gameObject.transform);
        _camera.m_Follow = _followTarget.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (_cameraDirector.WorldCamera.isActiveAndEnabled)
        {
            MatchReferenceCam();
            // check that the follow target exists, it hasn't changed or moved
            if (_cameraDirector.WorldCamera.m_Follow != null && _cameraDirector.WorldCamera.m_Follow == _savedCameraFollow && _cameraDirector.WorldCamera.m_Follow.position == _savedCameraFollowPosition)
            {
                Debug.Log("CheckPan");
                CheckPan();
            }
            else
            {
                Debug.Log($"_cameraDirector.WorldCamera.m_Follow: {_cameraDirector.WorldCamera.m_Follow}");
                Debug.Log($"_savedCameraFollow: {_savedCameraFollow}");
                Align();
                Deactivate();
            }
        }
        else
        {
            Align();
            Deactivate();
        }
    }

    void MatchReferenceCam()
    {
        // always match the rotation and offset of the reference camera
        _camera.transform.rotation = _cameraDirector.WorldCamera.transform.rotation;
        CinemachineComponentBase componentBase = _camera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        CinemachineComponentBase refComponentBase = _cameraDirector.WorldCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is CinemachineFramingTransposer && refComponentBase is CinemachineFramingTransposer)
        {
            (componentBase as CinemachineFramingTransposer).m_TrackedObjectOffset =
                (refComponentBase as CinemachineFramingTransposer).m_TrackedObjectOffset;
        }
    }

    void Align()
    {
        _offset = Vector3.zero;
        _savedCameraFollow = _cameraDirector.WorldCamera.m_Follow;
        if (_cameraDirector.WorldCamera.m_Follow != null)
        {
            _savedCameraFollowPosition = _cameraDirector.WorldCamera.m_Follow.position;
        }
        else
        {
            _savedCameraFollowPosition = Vector3.zero;
        }
    }

    void Deactivate()
    {
        _camera.gameObject.SetActive(false);
    }

    void CheckPan()
    {
        if (_useScreenBorders)
        {
            if (Input.mousePosition.y >= Screen.height - _panBorderThickness)
            {
                _offset = _offset + new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized * _panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.y < _panBorderThickness)
            {
                _offset = _offset - new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized * _panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x >= Screen.width - _panBorderThickness)
            {
                _offset = _offset + _camera.transform.right * _panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x < _panBorderThickness)
            {
                _offset = _offset - _camera.transform.right * _panSpeed * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            _offset = _offset + new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized * _panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _offset = _offset - new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized * _panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _offset = _offset + _camera.transform.right * _panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            _offset = _offset - _camera.transform.right * _panSpeed * Time.deltaTime;
        }
        _followTarget.transform.position = _savedCameraFollowPosition + _offset;
        if (_offset != Vector3.zero)
        {
            _camera.gameObject.SetActive(true);
        }
    }

    
}
