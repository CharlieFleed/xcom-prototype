using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _worldCamera;
    [SerializeField] CinemachineVirtualCamera _entityCamera;
    [SerializeField] CinemachineVirtualCamera _aimingCamera;
    [SerializeField] CinemachineVirtualCamera _overwatchCamera;
    [SerializeField] CinemachineVirtualCamera _actionCamera;
    [SerializeField] CinemachineTargetGroup _actionCameraTargetGroup;
    [SerializeField] CinemachineVirtualCamera _lowPriorityActionCamera;
    [SerializeField] CinemachineTargetGroup _lowPriorityActionCameraTargetGroup;
    [SerializeField] Transform _throwTarget;
    [SerializeField] Transform _worldCamTarget;

    List<Transform> _lowPriorityInvisibleTransfoms = new List<Transform>();

    Dictionary<GridEntity, CinemachineVirtualCamera> _entityCameras = new Dictionary<GridEntity, CinemachineVirtualCamera>();

    CinemachineVirtualCamera _activeEntityCamera;
    CinemachineVirtualCamera _savedEntityCamera;

    public CinemachineVirtualCamera WorldCamera { get { return _activeEntityCamera; } }

    public static event Action<bool> OnAimingCameraActiveChanged = delegate { };

    private void Awake()
    {
        SetAndActivateActiveEntityCamera(_worldCamera);
        _lowPriorityActionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
    }

    private void OnEnable()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
        UnitLocalController.OnActiveChanged += HandleUnit_OnActiveChanged;
        GridEntity.OnGridEntityAdded += HandleGridEntity_OnGridEntityAdded;
        GridEntity.OnGridEntityRemoved += HandleGridEntity_OnGridEntityRemoved;
        Shooter.OnShooterAdded += HandleShooterAdded;
        Shooter.OnShooterRemoved += HandleShooterRemoved;
        BattleEventShot.OnShooting += HandleBattleEventShot_OnShooting;
        BattleEventShot.OnShootingEnd += HandleBattleEventShot_OnShootingEnd;
        BattleEventThrow.OnThrowing += HandleBattleEventThrow_OnThrowing;
        BattleEventThrow.OnThrowingEnd += HandleBattleEventThrow_OnThrowingEnd;
        BattleEventExplosion.OnExploding += HandleBattleEventExplosion_OnExploding;
        BattleEventExplosion.OnExplodingEnd += HandleBattleEventExplosion_OnExplodingEnd;        
        Viewer.OnVisibleChanged += HandleViewer_OnVisibleChanged;
        BattleEventEngageAction.OnEngaging += HandleBattleEventEngageAction_OnEngaging;
        BattleEventEngageAction.OnEngagingEnd += HandleBattleEventEngageAction_OnEngagingEnd;
        BattleEventOverwatchShot.OnOverwatchShooting += HandleBattleEventOverwatchShot_OnOverwatchShooting;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
        UnitLocalController.OnActiveChanged -= HandleUnit_OnActiveChanged;
        GridEntity.OnGridEntityAdded -= HandleGridEntity_OnGridEntityAdded;
        GridEntity.OnGridEntityRemoved -= HandleGridEntity_OnGridEntityRemoved;
        Shooter.OnShooterAdded -= HandleShooterAdded;
        Shooter.OnShooterRemoved -= HandleShooterRemoved;
        BattleEventShot.OnShooting -= HandleBattleEventShot_OnShooting;
        BattleEventShot.OnShootingEnd -= HandleBattleEventShot_OnShootingEnd;
        BattleEventThrow.OnThrowing -= HandleBattleEventThrow_OnThrowing;
        BattleEventThrow.OnThrowingEnd -= HandleBattleEventThrow_OnThrowingEnd;
        BattleEventExplosion.OnExploding -= HandleBattleEventExplosion_OnExploding;
        BattleEventExplosion.OnExplodingEnd -= HandleBattleEventExplosion_OnExplodingEnd;        
        Viewer.OnVisibleChanged -= HandleViewer_OnVisibleChanged;
        BattleEventEngageAction.OnEngaging -= HandleBattleEventEngageAction_OnEngaging;
        BattleEventEngageAction.OnEngagingEnd -= HandleBattleEventEngageAction_OnEngagingEnd;
        BattleEventOverwatchShot.OnOverwatchShooting -= HandleBattleEventOverwatchShot_OnOverwatchShooting;
    }

    private void Start()
    {
        NetworkMatchManager.Instance.OnTurnBegin += HandleMatchManager_OnNewTurn;
    }

    private void OnDestroy()
    {
        NetworkMatchManager.Instance.OnTurnBegin -= HandleMatchManager_OnNewTurn;
    }

    private void Update()
    {
        foreach (var cam in _entityCameras.Values)
        {
            MatchReferenceCam(cam, _worldCamera);
        }
        MatchReferenceCam(_actionCamera, _worldCamera);
        MatchReferenceCam(_lowPriorityActionCamera, _worldCamera);
        //if (!_worldCamera.enabled)
        //    AlignCamera(_worldCamera, _activeEntityCamera);
    }

    void MatchReferenceCam(CinemachineVirtualCamera _camera, CinemachineVirtualCamera _referenceCamera)
    {
        // always match the rotation and offset of the reference camera
        _camera.transform.rotation = _referenceCamera.transform.rotation;
        CinemachineComponentBase componentBase = _camera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        CinemachineComponentBase refComponentBase = _referenceCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is CinemachineFramingTransposer && refComponentBase is CinemachineFramingTransposer)
        {
            (componentBase as CinemachineFramingTransposer).m_TrackedObjectOffset =
                (refComponentBase as CinemachineFramingTransposer).m_TrackedObjectOffset;
        }
    }

    void Handle_UnitLocalControllerAdded(UnitLocalController unit)
    {
        unit.OnMouseOverTarget += HandleUnit_OnMouseOverTarget;
        unit.OnMouseExitTarget += HandleUnit_OnMouseExitTarget;
    }

    void Handle_UnitLocalControllerRemoved(UnitLocalController unit)
    {
        unit.OnMouseOverTarget -= HandleUnit_OnMouseOverTarget;
        unit.OnMouseExitTarget -= HandleUnit_OnMouseExitTarget;
    }

    void HandleShooterAdded(Shooter shooter)
    {
        shooter.OnTargetSelected += HandleShooter_OnTargetSelected;
        shooter.OnTargetingEnd += HandleShooter_OnTargetingEnd;
    }

    void HandleShooterRemoved(Shooter shooter)
    {
        shooter.OnTargetSelected -= HandleShooter_OnTargetSelected;
        shooter.OnTargetingEnd -= HandleShooter_OnTargetingEnd;
    }

    void HandleGridEntity_OnGridEntityAdded(GridEntity target)
    {
        if (!_entityCameras.ContainsKey(target))
        {
            CinemachineVirtualCamera cam = GameObject.Instantiate(_entityCamera);
            cam.transform.SetParent(transform);
            cam.m_Follow = target.transform;
            _entityCameras.Add(target, cam);
        }
    }

    void HandleGridEntity_OnGridEntityRemoved(GridEntity target)
    {
        if (_entityCameras.ContainsKey(target) == true)
        {
            if (_entityCameras[target] != null)
            {
                if (_activeEntityCamera == _entityCameras[target])
                {
                    SetAndActivateActiveEntityCamera(_worldCamera);
                }
                Destroy(_entityCameras[target].gameObject);
            }
            _entityCameras.Remove(target);
        }
    }

    void HandleMatchManager_OnNewTurn()
    {
        Unit currentUnit = NetworkMatchManager.Instance.CurrentUnit;
        _actionCamera.gameObject.SetActive(false);
        if (currentUnit.GetComponent<Viewer>().IsVisible)
        {
            SetAndActivateActiveEntityCamera(_entityCameras[currentUnit.GetComponent<GridEntity>()]);
            // make the world cam follow this entity
            AttachWorldCamera(currentUnit.transform);
        }
        else
        {
            Debug.Log($"On new turn unit {currentUnit.name} is not visible");
            FixWorldCamera(_worldCamera.m_Follow.position);
        }
        // pre-align aiming camera with current unit
        _aimingCamera.m_Follow = currentUnit.transform;
        // pre-align action camera with current unit
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(currentUnit.transform, 1, 5); 
    }

    void HandleBattleEventShot_OnShooting(Shooter shooter, GridEntity target)
    {
        DisableActiveEntityCamera();
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(shooter.transform, 1, 5);
        _actionCameraTargetGroup.AddMember(target.transform, 1, 5);
    }

    void HandleBattleEventShot_OnShootingEnd(Shooter shooter, GridEntity target)
    {
        _overwatchCamera.gameObject.SetActive(false);
    }

    void HandleBattleEventThrow_OnThrowing(Thrower thrower, GridNode target)
    {
        DisableActiveEntityCamera();
        _throwTarget.transform.position = target.FloorPosition;
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(thrower.transform, 1, 5);
        _actionCameraTargetGroup.AddMember(_throwTarget.transform, 1, 5);
    }

    void HandleBattleEventThrow_OnThrowingEnd(Thrower thrower, GridNode target)
    {
    }

    void HandleBattleEventExplosion_OnExploding(GridEntity gridEntity)
    {
        DisableActiveEntityCamera();
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(gridEntity.transform, 1, 1);
    }

    void HandleBattleEventExplosion_OnExplodingEnd(GridEntity gridEntity)
    {
    }

    void HandleUnit_OnActiveChanged(UnitLocalController unit, bool active)
    {
        if (active) // NOTE: Only local units are activated
        {
            SetAndActivateActiveEntityCamera(_entityCameras[unit.GetComponent<GridEntity>()]);
            // make the world cam follow this entity
            AttachWorldCamera(unit.transform);
            _aimingCamera.m_Follow = unit.transform;
        }
    }

    void HandleUnit_OnMouseOverTarget(ShotStats target)
    {
        DisableActiveEntityCamera();
        _activeEntityCamera = _entityCameras[target.Target];
        _activeEntityCamera.gameObject.SetActive(true);
    }

    void HandleUnit_OnMouseExitTarget(ShotStats target)
    {
        DisableActiveEntityCamera();
        _activeEntityCamera = _savedEntityCamera;
        _activeEntityCamera.gameObject.SetActive(true);
    }

    void HandleShooter_OnTargetSelected(Shooter shooter, GridEntity target)
    {
        _aimingCamera.gameObject.SetActive(true);
        OnAimingCameraActiveChanged(true);
        _aimingCamera.LookAt = target.transform;
    }

    void HandleShooter_OnTargetingEnd()
    {
        _aimingCamera.gameObject.SetActive(false);
        OnAimingCameraActiveChanged(false);
        DisableActiveEntityCamera();
        _activeEntityCamera = _savedEntityCamera;
        _activeEntityCamera.gameObject.SetActive(true);
    }

    void HandleViewer_OnVisibleChanged(Viewer viewer, bool visible)
    {
        Unit currentUnit = NetworkMatchManager.Instance.CurrentUnit;
        if (visible)
        {
            // if this is the active unit who became visible
            if (currentUnit != null && currentUnit.GetComponent<GridEntity>() == viewer.GetComponent<GridEntity>())
            {
                //Debug.Log("this is the active unit who became visible");                
                // disable the active camera
                DisableActiveEntityCamera();
                SetAndActivateActiveEntityCamera(_entityCameras[currentUnit.GetComponent<GridEntity>()]);
                // make the world cam follow this entity
                AttachWorldCamera(currentUnit.transform);
            }
            // this is a unit that was followed by the low priority cam
            else if (_lowPriorityInvisibleTransfoms.Contains(viewer.transform))
            {
                _lowPriorityInvisibleTransfoms.Remove(viewer.transform);
                DisableActiveEntityCamera();
                _lowPriorityActionCameraTargetGroup.AddMember(viewer.transform, 1, 5);
                _lowPriorityActionCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            if (_activeEntityCamera == _entityCameras[viewer.GetComponent<GridEntity>()])
            {
                Debug.Log("this is the active unit who became invisible, switch to world camera");
                // this is the active unit who became invisible, switch to world camera
                DisableActiveEntityCamera();
                FixWorldCamera(viewer.transform.position);
            }
            // if this is a unit followed by the low priority cam
            else if (_lowPriorityActionCameraTargetGroup.FindMember(viewer.transform) > 0)
            {
                _lowPriorityActionCameraTargetGroup.RemoveMember(viewer.transform);
                _lowPriorityInvisibleTransfoms.Add(viewer.transform);
                if (_lowPriorityActionCameraTargetGroup.m_Targets.Length == 0)
                {
                    _lowPriorityActionCamera.gameObject.SetActive(false);
                }
            }
        }
    }

    void HandleBattleEventEngageAction_OnEngaging(GridEntity gridEntity)
    {
        DisableActiveEntityCamera();
        if (gridEntity.GetComponent<Viewer>().IsVisible)
        {
            _lowPriorityActionCameraTargetGroup.AddMember(gridEntity.transform, 1, 5);
            _lowPriorityActionCamera.gameObject.SetActive(true);
        }
        else
        {
            _lowPriorityInvisibleTransfoms.Add(gridEntity.transform);
        }
    }

    void HandleBattleEventEngageAction_OnEngagingEnd(GridEntity gridEntity)
    {
        if (_lowPriorityActionCameraTargetGroup.FindMember(gridEntity.transform) > 0)
            _lowPriorityActionCameraTargetGroup.RemoveMember(gridEntity.transform);
        if (_lowPriorityInvisibleTransfoms.Contains(gridEntity.transform))
            _lowPriorityInvisibleTransfoms.Remove(gridEntity.transform);
        if (_lowPriorityActionCameraTargetGroup.m_Targets.Length == 0)
        {
            _lowPriorityActionCamera.gameObject.SetActive(false);
        }
    }

    void HandleBattleEventOverwatchShot_OnOverwatchShooting(Shooter shooter, GridEntity target)
    {
        _overwatchCamera.m_Follow = shooter.transform;
        _overwatchCamera.LookAt = target.transform;
        _overwatchCamera.gameObject.SetActive(true);
    }

    void AlignCamera(CinemachineVirtualCamera camera, CinemachineVirtualCamera reference)
    {
        camera.transform.position = reference.transform.position;
        camera.transform.rotation = reference.transform.rotation;
        camera.transform.localScale = reference.transform.localScale;
    }

    void SetAndActivateActiveEntityCamera(CinemachineVirtualCamera camera)
    {
        DisableActiveEntityCamera();
        _activeEntityCamera = camera;
        _activeEntityCamera.gameObject.SetActive(true);
        if (_activeEntityCamera != _worldCamera)
            _savedEntityCamera = camera;
    }

    void DisableActiveEntityCamera()
    {
        // never disable the world camera
        if (_activeEntityCamera != null && _activeEntityCamera != _worldCamera)
        {
            _activeEntityCamera.gameObject.SetActive(false);
            _activeEntityCamera = _worldCamera;
        }
    }

    void FixWorldCamera(Vector3 position)
    {
        _worldCamera.m_Follow = _worldCamTarget;
        _worldCamera.m_Follow.position = position;
        AlignCamera(_worldCamera, _activeEntityCamera);
    }

    void AttachWorldCamera(Transform transform)
    {
        _worldCamera.m_Follow = transform;
    }
}
