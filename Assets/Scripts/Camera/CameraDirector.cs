//#define CAMERA_DIRECTOR_DEBUG

using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _worldCamera;
    [SerializeField] CinemachineVirtualCamera _entityCameraPrefab;
    [SerializeField] CinemachineVirtualCamera _aimingCamera;
    [SerializeField] CinemachineVirtualCamera _overwatchCamera;
    [SerializeField] CinemachineVirtualCamera _actionCamera;
    [SerializeField] CinemachineTargetGroup _actionCameraTargetGroup;
    [SerializeField] CinemachineVirtualCamera _lowPriorityActionCamera;
    [SerializeField] CinemachineTargetGroup _lowPriorityActionCameraTargetGroup;
    [SerializeField] Transform _throwTarget;
    [SerializeField] Transform _worldCamTarget;
    [SerializeField] CinemachineBrain _cinemachineBrain;

    [SerializeField] NetworkMatchManager _matchManager;

    List<Transform> _lowPriorityInvisibleTransfoms = new List<Transform>();

    Dictionary<GridEntity, CinemachineVirtualCamera> _entityCameras = new Dictionary<GridEntity, CinemachineVirtualCamera>();

    CinemachineVirtualCamera _thirdPersonCamera;
    CinemachineVirtualCamera _savedEntityCamera;

    public CinemachineVirtualCamera ThirdPersonCamera { get { return _thirdPersonCamera; } }

    public static event Action<bool> OnAimingCameraActiveChanged = delegate { };

    private void Awake()
    {
        SetAndActivateThirdPersonCamera(_worldCamera);
        _lowPriorityActionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
    }

    private void OnEnable()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
        GridEntity.OnGridEntityAdded += HandleGridEntity_OnGridEntityAdded;
        GridEntity.OnGridEntityRemoved += HandleGridEntity_OnGridEntityRemoved;
        Shooter.OnShooterAdded += HandleShooterAdded;
        Shooter.OnShooterRemoved += HandleShooterRemoved;
        BattleEventShot.OnShooting += HandleBattleEventShot_OnShooting;
        BattleEventShot.OnShootingEnd += HandleBattleEventShot_OnShootingEnd;
        BattleEventThrow.OnThrowing += HandleBattleEventThrow_OnThrowing;
        BattleEventThrow.OnThrowingEnd += HandleBattleEventThrow_OnThrowingEnd;
        BattleEventExplosion.OnExploding += HandleBattleEventExplosion_OnExploding;      
        Viewer.OnVisibleChanged += HandleViewer_OnVisibleChanged;
        BattleEventEngageAction.OnEngaging += HandleBattleEventEngageAction_OnEngaging;
        BattleEventEngageAction.OnEngagingEnd += HandleBattleEventEngageAction_OnEngagingEnd;
        BattleEventOverwatchShot.OnOverwatchShooting += HandleBattleEventOverwatchShot_OnOverwatchShooting;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
        GridEntity.OnGridEntityAdded -= HandleGridEntity_OnGridEntityAdded;
        GridEntity.OnGridEntityRemoved -= HandleGridEntity_OnGridEntityRemoved;
        Shooter.OnShooterAdded -= HandleShooterAdded;
        Shooter.OnShooterRemoved -= HandleShooterRemoved;
        BattleEventShot.OnShooting -= HandleBattleEventShot_OnShooting;
        BattleEventShot.OnShootingEnd -= HandleBattleEventShot_OnShootingEnd;
        BattleEventThrow.OnThrowing -= HandleBattleEventThrow_OnThrowing;
        BattleEventThrow.OnThrowingEnd -= HandleBattleEventThrow_OnThrowingEnd;
        BattleEventExplosion.OnExploding -= HandleBattleEventExplosion_OnExploding;    
        Viewer.OnVisibleChanged -= HandleViewer_OnVisibleChanged;
        BattleEventEngageAction.OnEngaging -= HandleBattleEventEngageAction_OnEngaging;
        BattleEventEngageAction.OnEngagingEnd -= HandleBattleEventEngageAction_OnEngagingEnd;
        BattleEventOverwatchShot.OnOverwatchShooting -= HandleBattleEventOverwatchShot_OnOverwatchShooting;
    }

    private void Start()
    {
        //Debug.Log("CameraDirector Start");
        _matchManager.OnTurnBegin += HandleMatchManager_OnNewTurn;
    }

    private void OnDestroy()
    {
        _matchManager.OnTurnBegin -= HandleMatchManager_OnNewTurn;
    }

    private void Update()
    {
        foreach (var cam in _entityCameras.Values)
        {
            AlignCamera(cam, _worldCamera);
        }
        AlignCamera(_actionCamera, _worldCamera);
        AlignCamera(_lowPriorityActionCamera, _worldCamera);
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log($"World Camera Position:({_worldCamera.transform.position.x},{_worldCamera.transform.position.y},{_worldCamera.transform.position.z})");
#endif
    }

    void AlignCamera(CinemachineVirtualCamera _camera, CinemachineVirtualCamera _referenceCamera)
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

    // Event Handlers

    void Handle_UnitLocalControllerAdded(UnitLocalController unit)
    {
        unit.OnMouseOverTarget += HandleUnitLocalController_OnMouseOverTarget;
        unit.OnMouseExitTarget += HandleUnitLocalController_OnMouseExitTarget;
    }

    void Handle_UnitLocalControllerRemoved(UnitLocalController unit)
    {
        unit.OnMouseOverTarget -= HandleUnitLocalController_OnMouseOverTarget;
        unit.OnMouseExitTarget -= HandleUnitLocalController_OnMouseExitTarget;
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
            CinemachineVirtualCamera cam = GameObject.Instantiate(_entityCameraPrefab);
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
                if (_thirdPersonCamera == _entityCameras[target])
                {
                    SetAndActivateThirdPersonCamera(_worldCamera);
                }
                Destroy(_entityCameras[target].gameObject);
            }
            _entityCameras.Remove(target);
        }
    }

    //

    void HandleMatchManager_OnNewTurn()
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("HandleMatchManager_OnNewTurn");
#endif

        Unit currentUnit = NetworkMatchManager.Instance.CurrentUnit;
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Disactivating _actionCamera");
#endif
        _actionCamera.gameObject.SetActive(false);
        if (currentUnit.GetComponent<Viewer>().IsVisible)
        {
            SetAndActivateThirdPersonCamera(_entityCameras[currentUnit.GetComponent<GridEntity>()]);
            // make the world cam follow this entity
            //AttachWorldCameraToTarget(currentUnit.transform);
        }
        else
        {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log($"On new turn unit {currentUnit.name} is not visible");
#endif
            FixWorldCameraToPosition(_worldCamera.m_Follow.position);
        }
        // pre-align aiming camera with current unit
        _aimingCamera.m_Follow = currentUnit.transform;
    }

    void HandleBattleEventShot_OnShooting(Shooter shooter, GridEntity target)
    {
        DisableThirdPersonCamera();
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _actionCamera");
#endif
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(shooter.transform, 1, 5);
        _actionCameraTargetGroup.AddMember(target.transform, 1, 5);
    }

    void HandleBattleEventShot_OnShootingEnd(Shooter shooter, GridEntity target)
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log($"CameraDirector.HandleBattleEventShot_OnShootingEnd");
        Debug.Log("Disactivating _overwatchCamera");
#endif
        _overwatchCamera.gameObject.SetActive(false);
    }

    void HandleBattleEventThrow_OnThrowing(Thrower thrower, GridNode target)
    {
        DisableThirdPersonCamera();
        _throwTarget.transform.position = target.FloorPosition;
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _actionCamera");
#endif
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
        DisableThirdPersonCamera();
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _actionCamera");
#endif
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(gridEntity.transform, 1, 1);
    }

    void HandleUnitLocalController_OnMouseOverTarget(ShotStats target)
    {
        DisableThirdPersonCamera();
        _thirdPersonCamera = _entityCameras[target.Target];
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _thirdPersonCamera");
#endif
        _thirdPersonCamera.gameObject.SetActive(true);
    }

    void HandleUnitLocalController_OnMouseExitTarget(ShotStats target)
    {
        DisableThirdPersonCamera();
        _thirdPersonCamera = _savedEntityCamera;
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _thirdPersonCamera");
#endif
        _thirdPersonCamera.gameObject.SetActive(true);
    }

    void HandleShooter_OnTargetSelected(Shooter shooter, GridEntity target)
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _aimingCamera");
#endif
        _aimingCamera.gameObject.SetActive(true);
        OnAimingCameraActiveChanged(true);
        _aimingCamera.LookAt = target.transform;
    }

    void HandleShooter_OnTargetingEnd()
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Disactivating _aimingCamera");
#endif
        _aimingCamera.gameObject.SetActive(false);
        OnAimingCameraActiveChanged(false);
        DisableThirdPersonCamera();
        _thirdPersonCamera = _savedEntityCamera;
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _thirdPersonCamera");
#endif
        _thirdPersonCamera.gameObject.SetActive(true);
    }

    void HandleViewer_OnVisibleChanged(Viewer viewer, bool visible)
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log($"HandleViewer_OnVisibleChanged({viewer.name},{visible})");
#endif
        Unit currentUnit = NetworkMatchManager.Instance.CurrentUnit;
        if (visible)
        {
            // if this is the current unit who became visible
            if (currentUnit != null && currentUnit.GetComponent<GridEntity>() == viewer.GetComponent<GridEntity>())
            {
#if CAMERA_DIRECTOR_DEBUG
                Debug.Log("this is the active unit who became visible");   
#endif
                // disable the active camera
                DisableThirdPersonCamera();
                SetAndActivateThirdPersonCamera(_entityCameras[currentUnit.GetComponent<GridEntity>()]);
                // make the world cam follow this entity
                AttachWorldCameraToTarget(currentUnit.transform);
            }
            // this is a unit that was followed by the low priority cam
            else if (_lowPriorityInvisibleTransfoms.Contains(viewer.transform))
            {
                _lowPriorityInvisibleTransfoms.Remove(viewer.transform);
#if CAMERA_DIRECTOR_DEBUG
                Debug.Log($"_lowPriorityInvisibleTransfoms.Count={_lowPriorityInvisibleTransfoms.Count}");
#endif
                _lowPriorityActionCameraTargetGroup.AddMember(viewer.transform, 1, 5);
#if CAMERA_DIRECTOR_DEBUG
                Debug.Log($"_lowPriorityActionCameraTargetGroup.m_Targets.Length={_lowPriorityActionCameraTargetGroup.m_Targets.Length}");
#endif
                DisableThirdPersonCamera();
#if CAMERA_DIRECTOR_DEBUG
                Debug.Log("Activating _lowPriorityActionCamera");
#endif
                _lowPriorityActionCamera.gameObject.SetActive(true);
            }
        }
        else
        {
#if CAMERA_DIRECTOR_DEBUG
            Debug.Log("_lowPriorityActionCameraTargetGroup:");
            foreach (CinemachineTargetGroup.Target t in _lowPriorityActionCameraTargetGroup.m_Targets)
            {
                Debug.Log(t.target.name);
            }
#endif
            if (_thirdPersonCamera == _entityCameras[viewer.GetComponent<GridEntity>()])
            {
#if CAMERA_DIRECTOR_DEBUG
                Debug.Log("this is the active unit who became invisible, switch to world camera");
#endif
                // this is the active unit who became invisible, switch to world camera
                DisableThirdPersonCamera();
                //FixWorldCamera(viewer.transform.position);
                FixWorldCameraToPosition(viewer.GetComponent<Movement>().TargetPosition);
            }
            // if this is a unit followed by the low priority cam
            else if (_lowPriorityActionCameraTargetGroup.FindMember(viewer.transform) > -1)
            {
                _lowPriorityActionCameraTargetGroup.RemoveMember(viewer.transform);
                //Debug.Log($"_lowPriorityActionCameraTargetGroup.m_Targets.Length={_lowPriorityActionCameraTargetGroup.m_Targets.Length}");
                _lowPriorityInvisibleTransfoms.Add(viewer.transform);
                //Debug.Log($"_lowPriorityInvisibleTransfoms.Count={_lowPriorityInvisibleTransfoms.Count}");
                if (_lowPriorityActionCameraTargetGroup.m_Targets.Length == 0)
                {
#if CAMERA_DIRECTOR_DEBUG
                    Debug.Log("Disactivating _lowPriorityActionCamera");
#endif
                    _lowPriorityActionCamera.gameObject.SetActive(false);
                }
            }
        }
    }

    void HandleBattleEventEngageAction_OnEngaging(GridEntity gridEntity)
    {
        DisableThirdPersonCamera();
        if (gridEntity.GetComponent<Viewer>().IsVisible)
        {
            _lowPriorityActionCameraTargetGroup.AddMember(gridEntity.transform, 1, 5);
#if CAMERA_DIRECTOR_DEBUG
            Debug.Log("Activating _lowPriorityActionCamera");
#endif
            _lowPriorityActionCamera.gameObject.SetActive(true);
        }
        else
        {
            _lowPriorityInvisibleTransfoms.Add(gridEntity.transform);
            //Debug.Log($"_lowPriorityInvisibleTransfoms.Count={_lowPriorityInvisibleTransfoms.Count}");
        }
    }

    void HandleBattleEventEngageAction_OnEngagingEnd(GridEntity gridEntity)
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log($"HandleBattleEventEngageAction_OnEngagingEnd({gridEntity.name})");
#endif
        if (_lowPriorityActionCameraTargetGroup.FindMember(gridEntity.transform) > -1)
        {
            _lowPriorityActionCameraTargetGroup.RemoveMember(gridEntity.transform);
            //Debug.Log($"_lowPriorityActionCameraTargetGroup.m_Targets.Length={_lowPriorityActionCameraTargetGroup.m_Targets.Length}");
        }
        if (_lowPriorityInvisibleTransfoms.Contains(gridEntity.transform))
        {
            _lowPriorityInvisibleTransfoms.Remove(gridEntity.transform);
            //Debug.Log($"_lowPriorityInvisibleTransfoms.Count={_lowPriorityInvisibleTransfoms.Count}");
        }
        if (_lowPriorityActionCameraTargetGroup.m_Targets.Length == 0)
        {
#if CAMERA_DIRECTOR_DEBUG
            Debug.Log("Disactivating _lowPriorityActionCamera");
#endif
            _lowPriorityActionCamera.gameObject.SetActive(false);
        }
    }

    void HandleBattleEventOverwatchShot_OnOverwatchShooting(Shooter shooter, GridEntity target)
    {
        _overwatchCamera.m_Follow = shooter.transform;
        _overwatchCamera.LookAt = target.transform;
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _overwatchCamera");
#endif
        _overwatchCamera.gameObject.SetActive(true);
    }

    //

    void MatchCamera(CinemachineVirtualCamera camera, CinemachineVirtualCamera reference)
    {
        camera.transform.position = reference.transform.position;
        camera.transform.rotation = reference.transform.rotation;
        camera.transform.localScale = reference.transform.localScale;
    }

    void SetAndActivateThirdPersonCamera(CinemachineVirtualCamera camera)
    {
        DisableThirdPersonCamera();
        _thirdPersonCamera = camera;
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log("Activating _thirdPersonCamera");
#endif
        _thirdPersonCamera.gameObject.SetActive(true);
        if (_thirdPersonCamera != _worldCamera)
            _savedEntityCamera = camera;
    }

    void DisableThirdPersonCamera()
    {
        // never disable the world camera
        if (_thirdPersonCamera != null && _thirdPersonCamera != _worldCamera)
        {
#if CAMERA_DIRECTOR_DEBUG
            Debug.Log("Disactivating _thirdPersonCamera");
#endif
            _thirdPersonCamera.gameObject.SetActive(false);
            FixWorldCameraToPosition(_thirdPersonCamera.m_Follow.position);
            _thirdPersonCamera = _worldCamera;
        }
    }

    void FixWorldCameraToPosition(Vector3 targetPosition)
    {
#if CAMERA_DIRECTOR_DEBUG
        Debug.Log($"FixWorldCameraToPosition({position.x},{position.y},{position.z})");
#endif
        _worldCamTarget.position = targetPosition;
        _worldCamera.m_Follow = _worldCamTarget;
        MatchCamera(_worldCamera, _thirdPersonCamera);
    }

    void AttachWorldCameraToTarget(Transform transform)
    {
        //Debug.Log($"AttachWorldCamera({transform.name})");
        _worldCamera.m_Follow = transform;
    }
}
