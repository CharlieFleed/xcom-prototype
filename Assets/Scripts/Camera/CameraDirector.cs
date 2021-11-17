using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _worldCamera;
    [SerializeField] CinemachineVirtualCamera _aimingCamera;
    [SerializeField] CinemachineVirtualCamera _actionCamera;
    [SerializeField] CinemachineTargetGroup _actionCameraTargetGroup;
    [SerializeField] NetworkMatchManager _matchManager;
    [SerializeField] Transform _throwTarget;

    Dictionary<GridEntity, CinemachineVirtualCamera> _entityCameras = new Dictionary<GridEntity, CinemachineVirtualCamera>();

    CinemachineVirtualCamera _activeEntityCamera;
    CinemachineVirtualCamera _lastActivatedUnitCamera;

    public CinemachineVirtualCamera WorldCamera { get { return _activeEntityCamera; } }

    public static event Action<bool> OnAimingCameraActiveChanged = delegate { };

    private void Awake()
    {
        _activeEntityCamera = _worldCamera;
        _lastActivatedUnitCamera = _worldCamera;
    }

    private void OnEnable()
    {
        Unit.OnUnitAdded += HandleUnit_OnUnitAdded;
        Unit.OnUnitRemoved += HandleUnit_OnUnitRemoved;
        Unit.OnActiveChanged += HandleUnit_OnActiveChanged;
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
        _matchManager.OnTurnBegin += HandleMatchManager_OnNewTurn;
        Viewer.OnVisibleChanged += HandleViewer_OnVisibleChanged;
    }
    
    private void OnDisable()
    {
        Unit.OnUnitAdded -= HandleUnit_OnUnitAdded;
        Unit.OnUnitRemoved -= HandleUnit_OnUnitRemoved;
        Unit.OnActiveChanged -= HandleUnit_OnActiveChanged;
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
        _matchManager.OnTurnBegin -= HandleMatchManager_OnNewTurn;
        Viewer.OnVisibleChanged -= HandleViewer_OnVisibleChanged;
    }

    private void Update()
    {
        foreach (var cam in _entityCameras.Values)
        {
            MatchReferenceCam(cam, _worldCamera);
        }
        MatchReferenceCam(_actionCamera, _worldCamera);
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

    void HandleUnit_OnUnitAdded(Unit unit)
    {
        unit.OnMouseOverTarget += HandleUnit_OnMouseOverTarget;
        unit.OnMouseExitTarget += HandleUnit_OnMouseExitTarget;
    }

    void HandleUnit_OnUnitRemoved(Unit unit)
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

    /// <summary>
    /// Add the entity camera.
    /// </summary>
    /// <param name="target"></param>
    void HandleGridEntity_OnGridEntityAdded(GridEntity target)
    {
        if (!_entityCameras.ContainsKey(target))
        {
            CinemachineVirtualCamera cam = GameObject.Instantiate(_worldCamera);
            cam.transform.SetParent(transform);
            cam.m_Follow = target.transform;
            cam.gameObject.SetActive(false);
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
                    _activeEntityCamera = _worldCamera;
                    _lastActivatedUnitCamera = _worldCamera;
                }
                Destroy(_entityCameras[target].gameObject);
            }
            _entityCameras.Remove(target);
        }
    }

    void HandleMatchManager_OnNewTurn()
    {
        // disable any active camera
        _actionCamera.gameObject.SetActive(false);
        _activeEntityCamera.gameObject.SetActive(false);
        if (_matchManager.CurrentUnit.GetComponent<Viewer>().IsVisible)
        {
            _activeEntityCamera = _entityCameras[_matchManager.CurrentUnit.GetComponent<GridEntity>()];
            _activeEntityCamera.gameObject.SetActive(true);
            _lastActivatedUnitCamera = _activeEntityCamera;
        }
        else
        {
            //Debug.Log($"Unit {_matchManager.CurrentUnit.name} is not visible");
            // align world camera with last entity
            _worldCamera.m_Follow.position = _activeEntityCamera.m_Follow.position;
            _activeEntityCamera = _worldCamera;
            _activeEntityCamera.gameObject.SetActive(true);
            _lastActivatedUnitCamera = _activeEntityCamera;
        }
        _aimingCamera.m_Follow = _matchManager.CurrentUnit.transform;
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(_matchManager.CurrentUnit.transform, 1, 5); // pre-align action camera with current unit
    }

    void HandleBattleEventShot_OnShooting(Shooter shooter, GridEntity target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(shooter.transform, 1, 5);
        _actionCameraTargetGroup.AddMember(target.transform, 1, 5);
    }

    void HandleBattleEventShot_OnShootingEnd()
    {
    }

    void HandleBattleEventThrow_OnThrowing(Thrower thrower, GridNode target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(thrower.transform, 1, 5);
        _throwTarget.transform.position = target.FloorPosition;
        _actionCameraTargetGroup.AddMember(_throwTarget.transform, 1, 5);
    }

    void HandleBattleEventThrow_OnThrowingEnd()
    {
    }

    void HandleBattleEventExplosion_OnExploding(GridEntity gridEntity)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _actionCamera.gameObject.SetActive(true);
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(gridEntity.transform, 1, 1);
    }

    void HandleBattleEventExplosion_OnExplodingEnd()
    {
    }

    void HandleUnit_OnActiveChanged(Unit unit, bool active)
    {
        if (active) // NOTE: Only local units are activated
        {
            // disable any active camera
            _activeEntityCamera.gameObject.SetActive(false);
            //
            _activeEntityCamera = _entityCameras[unit.GetComponent<GridEntity>()];
            _activeEntityCamera.gameObject.SetActive(true);
            _lastActivatedUnitCamera = _activeEntityCamera;
            _aimingCamera.m_Follow = unit.transform;
        }
    }

    void HandleUnit_OnMouseOverTarget(ShotStats target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _activeEntityCamera = _entityCameras[target.Target];
        _activeEntityCamera.gameObject.SetActive(true);
    }

    void HandleUnit_OnMouseExitTarget(ShotStats target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _activeEntityCamera = _lastActivatedUnitCamera;
        _lastActivatedUnitCamera.gameObject.SetActive(true);
    }

    void HandleShooter_OnTargetSelected(Shooter shooter, GridEntity target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _aimingCamera.gameObject.SetActive(true);
        OnAimingCameraActiveChanged(true);
        _aimingCamera.LookAt = target.transform;
    }

    void HandleShooter_OnTargetingEnd()
    {
        _aimingCamera.gameObject.SetActive(false);
        OnAimingCameraActiveChanged(false);
        _activeEntityCamera = _lastActivatedUnitCamera;
        _lastActivatedUnitCamera.gameObject.SetActive(true);
    }

    void HandleViewer_OnVisibleChanged(Viewer viewer, bool visible)
    {
        if (visible)
        {
            if (_matchManager.CurrentUnit != null && _matchManager.CurrentUnit.GetComponent<GridEntity>() == viewer.GetComponent<GridEntity>())
            {
                //Debug.Log("this is the active unit who became visible");
                // this is the active unit who became visible
                // disable any active camera
                _activeEntityCamera.gameObject.SetActive(false);
                //
                _activeEntityCamera = _entityCameras[_matchManager.CurrentUnit.GetComponent<GridEntity>()];
                _activeEntityCamera.gameObject.SetActive(true);
                _lastActivatedUnitCamera = _activeEntityCamera;
            }
        }
        else
        {
            if (_activeEntityCamera == _entityCameras[viewer.GetComponent<GridEntity>()])
            {
                //Debug.Log("this is the active unit who became invisible, switch to world camera");
                // this is the active unit who became invisible, switch to world camera
                _activeEntityCamera.gameObject.SetActive(false);
                // align world camera with last entity
                _worldCamera.m_Follow.position = _activeEntityCamera.m_Follow.position;
                AlignCamera(_worldCamera, _activeEntityCamera);
                _activeEntityCamera = _worldCamera;
                _activeEntityCamera.gameObject.SetActive(true);
                _lastActivatedUnitCamera = _activeEntityCamera;
            }
        }
    }

    void AlignCamera(CinemachineVirtualCamera camera, CinemachineVirtualCamera reference)
    {
        camera.transform.position = reference.transform.position;
        camera.transform.rotation = reference.transform.rotation;
        camera.transform.localScale = reference.transform.localScale;
    }
}
