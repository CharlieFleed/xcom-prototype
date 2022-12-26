using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _virtualCamera;
    [SerializeField] CameraDirector _cameraDirector;
    [SerializeField] float _rotationSpeed = 2f;
    [SerializeField] float _smallRotationSpeed = 25f;

    public int Level
    {
        get { return _level; }
        private set { if (value != _level) { _level = value; OnLevelChanged(); } }
    }

    int _level = 0;
    int _levelOffset = 0;
    public event Action OnLevelChanged = delegate { };

    Transform _savedTarget;
    int _savedTargetLevel = 0;
    int _maxLevel = 5;

    Quaternion _initialRotation;
    int _orientation = 0;

    Quaternion _from;
    Quaternion _to;
    float _t = 1f;

    private void Awake()
    {
        _initialRotation = _virtualCamera.transform.rotation;
        _orientation = 0;

        _from = _virtualCamera.transform.rotation;
        _to = _virtualCamera.transform.rotation;
        _instance = this;
    }

    private void OnEnable()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionActivated += HandleActionActivated;
        unitLocalController.OnActionConfirmed += HandleActionConfirmed;
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionActivated -= HandleActionActivated;
        unitLocalController.OnActionConfirmed -= HandleActionConfirmed;
    }

    private void HandleActionActivated(BattleAction battleAction)
    {
        ResetLevel();
    }    

    private void HandleActionConfirmed()
    {
        ResetLevel();
    }

    /// <summary>
    /// Align the camera level to the target.
    /// </summary>
    private void ResetLevel()
    {
        if (_cameraDirector.ThirdPersonCamera.m_Follow != null)
        {
            Level = GridManager.Instance.GetGridNodeFromWorldPosition(_cameraDirector.ThirdPersonCamera.m_Follow.transform.position).Y;
            _savedTargetLevel = Level;
            _levelOffset = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTarget();
        UpdateLevel();
        UpdateRotation();
    }

    private void UpdateLevel()
    {
        if (Input.mouseScrollDelta.y < 0)
        {
            Level = Mathf.Clamp(Level - 1, 0, _maxLevel);
            if (_savedTarget != null)
            {
                _levelOffset = Level - GridManager.Instance.GetGridNodeFromWorldPosition(_savedTarget.position).Y;
            }
        }
        if (Input.mouseScrollDelta.y > 0)
        {
            Level = Mathf.Clamp(Level + 1, 0, _maxLevel);
            if (_savedTarget != null)
            {
                _levelOffset = Level - GridManager.Instance.GetGridNodeFromWorldPosition(_savedTarget.position).Y;
            }
        }

        CinemachineComponentBase componentBase = _virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is CinemachineFramingTransposer)
        {
            (componentBase as CinemachineFramingTransposer).m_TrackedObjectOffset.y = _levelOffset * GridManager.Instance.YScale;
        }
    }

    private void UpdateTarget()
    {
        if (_cameraDirector.ThirdPersonCamera.m_Follow != null)
        {
            // if the follow target changed, align with it
            if (_cameraDirector.ThirdPersonCamera.m_Follow != _savedTarget)
            {
                //Debug.Log($"CameraController.UpdateTarget - follow target changed, camera: {_cameraDirector.ThirdPersonCamera.name}, old target: {_savedTarget?.name}, new target: {_cameraDirector.ThirdPersonCamera.m_Follow?.name}");
                Level = GridManager.Instance.GetGridNodeFromWorldPosition(_cameraDirector.ThirdPersonCamera.m_Follow.transform.position).Y;
                _savedTargetLevel = Level;
                _levelOffset = 0;
            }
            // if the follow target changed level
            else if (GridManager.Instance.GetGridNodeFromWorldPosition(_savedTarget.position).Y != _savedTargetLevel)
            {
                Level = GridManager.Instance.GetGridNodeFromWorldPosition(_savedTarget.position).Y;
                _savedTargetLevel = Level;
            }
        }
        else
        {
            Level = 0;
            _levelOffset = 0;
        }
        _savedTarget = _cameraDirector.ThirdPersonCamera.m_Follow;
    }

    private void UpdateRotation()
    {
        _t += Time.deltaTime * _rotationSpeed;
        _virtualCamera.transform.rotation = Quaternion.Lerp(_from, _to, _t);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateLeft();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateRight();
        }
        if (Input.GetKey(KeyCode.Z))
        {
            RotateLeftSmall();
        }
        if (Input.GetKey(KeyCode.C))
        {
            RotateRightSmall();
        }
    }

    public void RotateLeft()
    {
        _from = _virtualCamera.transform.rotation;
        _orientation = (_orientation + 1) % 4;
        Quaternion rotation = Quaternion.AngleAxis(_orientation * 90, new Vector3(0, 1, -1).normalized);
        _to = _initialRotation * rotation;
        _t = 0;
    }

    public void RotateRight()
    {
        _from = _virtualCamera.transform.rotation;
        _orientation = (_orientation + 3) % 4;
        Quaternion rotation = Quaternion.AngleAxis(_orientation * 90, new Vector3(0, 1, -1).normalized);
        _to = _initialRotation * rotation;
        _t = 0;
    }

    public void RotateLeftSmall()
    {
        Quaternion from = _virtualCamera.transform.rotation;
        Quaternion rotation = Quaternion.AngleAxis(_smallRotationSpeed * Time.deltaTime, new Vector3(0, 1, -1).normalized);
        _virtualCamera.transform.rotation = from * rotation;
        CancelStepRotation();
    }

    public void RotateRightSmall()
    {
        Quaternion from = _virtualCamera.transform.rotation;
        Quaternion rotation = Quaternion.AngleAxis(-_smallRotationSpeed * Time.deltaTime, new Vector3(0, 1, -1).normalized);
        _virtualCamera.transform.rotation = from * rotation;
        CancelStepRotation();
    }

    void CancelStepRotation()
    {
        _from = _virtualCamera.transform.rotation;
        _to = _virtualCamera.transform.rotation;
    }

    #region Singleton

    private static CameraController _instance;

    public static CameraController Instance { get { return _instance; } }

    #endregion
}
