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

    Quaternion _from;
    Quaternion _to;
    float _speed = 2f;
    float _t = 1f;

    private void Awake()
    {
        _from = _virtualCamera.transform.rotation;
        _to = _virtualCamera.transform.rotation;
        _instance = this;
        Character.OnCharacterAdded += HandleCharacterAdded;
        Character.OnCharacterRemoved += HandleCharacterRemoved;
    }

    private void HandleCharacterAdded(Character character)
    {
        character.OnActionActivated += HandleActionActivated;
        character.OnActionConfirmed += HandleActionConfirmed;
    }

    private void HandleCharacterRemoved(Character character)
    {
        character.OnActionActivated -= HandleActionActivated;
        character.OnActionConfirmed -= HandleActionConfirmed;
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
    }

    public void RotateLeft()
    {
        _from = _virtualCamera.transform.rotation;
        Quaternion rotation = Quaternion.AngleAxis(90, new Vector3(0, 1, -1).normalized);
        _to = _to * rotation;
        _t = 0;
    }
    public void RotateRight()
    {
        _from = _virtualCamera.transform.rotation;
        Quaternion rotation = Quaternion.AngleAxis(-90, new Vector3(0, 1, -1).normalized);
        _to = _to * rotation;
        _t = 0;
    }

    private void OnDestroy()
    {
        Character.OnCharacterAdded -= HandleCharacterAdded;
        Character.OnCharacterRemoved -= HandleCharacterRemoved;
    }

    #region Singleton

    private static CameraController _instance;

    public static CameraController Instance { get { return _instance; } }

    #endregion
}
