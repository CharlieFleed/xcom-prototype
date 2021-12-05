using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _virtualCamera;
    [SerializeField] CameraDirector _cameraDirector;

    public int Level { get { return _level; } private set { if (value != _level) { _level = value; OnLevelChanged(); } } }

    int _level = 0;
    int _levelOffset = 0;
    public event Action OnLevelChanged = delegate { };

    Transform _savedTarget;
    int _savedTargetLevel = 0;
    int _maxLevel = 5;

    Quaternion _initial;
    int _orientation = 0;

    Quaternion _from;
    Quaternion _to;
    float _speed = 2f;
    float _t = 1f;

    float _smallSpeed = 25f;

    private void Awake()
    {
        _initial = _virtualCamera.transform.rotation;
        _orientation = 0;

        _from = _virtualCamera.transform.rotation;
        _to = _virtualCamera.transform.rotation;
        _instance = this;
    }

    private void OnEnable()
    {
        UnitLocalController.OnUnitAdded += HandleUnitAdded;
        UnitLocalController.OnUnitRemoved += HandleUnitRemoved;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitAdded -= HandleUnitAdded;
        UnitLocalController.OnUnitRemoved -= HandleUnitRemoved;
    }

    private void HandleUnitAdded(UnitLocalController unit)
    {
        unit.OnActionActivated += HandleActionActivated;
        unit.OnActionConfirmed += HandleActionConfirmed;
    }

    private void HandleUnitRemoved(UnitLocalController unit)
    {
        unit.OnActionActivated -= HandleActionActivated;
        unit.OnActionConfirmed -= HandleActionConfirmed;
    }

    private void HandleActionActivated(BattleAction battleAction)
    {
        if (_cameraDirector.WorldCamera.m_Follow != null)
        {
            Level = GridManager.Instance.GetGridNodeFromWorldPosition(_cameraDirector.WorldCamera.m_Follow.transform.position).Y;
            _savedTargetLevel = Level;
            _levelOffset = 0;
        }
    }

    private void HandleActionConfirmed()
    {
        if (_cameraDirector.WorldCamera.m_Follow != null)
        {
            Level = GridManager.Instance.GetGridNodeFromWorldPosition(_cameraDirector.WorldCamera.m_Follow.transform.position).Y;
            _savedTargetLevel = Level;
            _levelOffset = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_cameraDirector.WorldCamera.m_Follow != null)
        {
            // if the follow target changed, align with it
            if (_cameraDirector.WorldCamera.m_Follow != _savedTarget)
            {
                Level = GridManager.Instance.GetGridNodeFromWorldPosition(_cameraDirector.WorldCamera.m_Follow.transform.position).Y;
                _savedTargetLevel = Level;
                _levelOffset = 0;
            }
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
        _savedTarget = _cameraDirector.WorldCamera.m_Follow;

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

        UpdateRotation();
    }

    private void UpdateRotation()
    {
        _t += Time.deltaTime * _speed;
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
        _to = _initial * rotation;
        _t = 0;
    }

    public void RotateRight()
    {
        _from = _virtualCamera.transform.rotation;
        _orientation = (_orientation + 3) % 4;
        Quaternion rotation = Quaternion.AngleAxis(_orientation * 90, new Vector3(0, 1, -1).normalized);
        _to = _initial * rotation;
        _t = 0;
    }

    public void RotateLeftSmall()
    {
        Quaternion from = _virtualCamera.transform.rotation;
        Quaternion rotation = Quaternion.AngleAxis(_smallSpeed * Time.deltaTime, new Vector3(0, 1, -1).normalized);
        _virtualCamera.transform.rotation = from * rotation;
        CancelStepRotation();
    }

    public void RotateRightSmall()
    {
        Quaternion from = _virtualCamera.transform.rotation;
        Quaternion rotation = Quaternion.AngleAxis(-_smallSpeed * Time.deltaTime, new Vector3(0, 1, -1).normalized);
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
