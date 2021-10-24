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

    Dictionary<GridEntity, CinemachineVirtualCamera> _entityCameras = new Dictionary<GridEntity, CinemachineVirtualCamera>();

    CinemachineVirtualCamera _activeEntityCamera;
    CinemachineVirtualCamera _lastActivatedCharacterCamera;

    public CinemachineVirtualCamera WorldCamera { get { return _activeEntityCamera; } }

    public static event Action<bool> OnAimingCameraActiveChanged = delegate { };

    private void Awake()
    {
        _activeEntityCamera = _worldCamera;
        _lastActivatedCharacterCamera = _worldCamera;
        //
        _matchManager.OnNewTurn += HandleMatchManager_OnNewTurn;
    }

    private void OnEnable()
    {
        Character.OnCharacterAdded += HandleCharacterAdded;
        Character.OnCharacterRemoved += HandleCharacterRemoved;
        Character.OnActiveChanged += HandleCharacter_OnActiveChanged;
        GridEntity.OnGridEntityAdded += HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved += HandleGridEntityRemoved;
        Shooter.OnShooterAdded += HandleShooterAdded;
        Shooter.OnShooterRemoved += HandleShooterRemoved;
        BattleEventShot.OnShooting += HandleBattleEventShot_OnShooting;
        BattleEventShot.OnShootingEnd += HandleBattleEventShot_OnShootingEnd;
        BattleEventExplosion.OnExploding += HandleBattleEventExplosion_OnExploding;
        BattleEventExplosion.OnExplodingEnd += HandleBattleEventExplosion_OnExplodingEnd;
    }

    private void OnDisable()
    {
        Character.OnCharacterAdded -= HandleCharacterAdded;
        Character.OnCharacterRemoved -= HandleCharacterRemoved;
        Character.OnActiveChanged -= HandleCharacter_OnActiveChanged;
        GridEntity.OnGridEntityAdded -= HandleGridEntityAdded;
        GridEntity.OnGridEntityRemoved -= HandleGridEntityRemoved;
        Shooter.OnShooterAdded -= HandleShooterAdded;
        Shooter.OnShooterRemoved -= HandleShooterRemoved;
        BattleEventShot.OnShooting -= HandleBattleEventShot_OnShooting;
        BattleEventShot.OnShootingEnd -= HandleBattleEventShot_OnShootingEnd;
    }

    private void HandleMatchManager_OnNewTurn()
    {
        // disable any active camera
        _actionCamera.gameObject.SetActive(false);
        _activeEntityCamera.gameObject.SetActive(false);
        //
        _activeEntityCamera = _entityCameras[_matchManager.CurrentCharacter.GetComponent<GridEntity>()];
        _activeEntityCamera.gameObject.SetActive(true);
        _lastActivatedCharacterCamera = _activeEntityCamera;
        _aimingCamera.m_Follow = _matchManager.CurrentCharacter.transform;
        _actionCameraTargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        _actionCameraTargetGroup.AddMember(_matchManager.CurrentCharacter.transform, 1, 5); // pre-align action camera with current character
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

    private void Update()
    {
        foreach (var cam in _entityCameras.Values)
        {
            MatchReferenceCam(cam, _worldCamera);
        }
        MatchReferenceCam(_actionCamera, _worldCamera);
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

    void HandleCharacterAdded(Character character)
    {
        character.OnMouseOverTarget += HandleMouseOverTarget;
        character.OnMouseExitTarget += HandleMouseExitTarget;
    }

    void HandleCharacterRemoved(Character character)
    {
        character.OnMouseOverTarget -= HandleMouseOverTarget;
        character.OnMouseExitTarget -= HandleMouseExitTarget;
    }

    void HandleGridEntityAdded(GridEntity target)
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

    void HandleGridEntityRemoved(GridEntity target)
    {
        if (_entityCameras.ContainsKey(target) == true)
        {
            if (_entityCameras[target] != null)
            {
                if (_activeEntityCamera == _entityCameras[target])
                {
                    _activeEntityCamera = _worldCamera;
                    _lastActivatedCharacterCamera = _worldCamera;
                }
                Destroy(_entityCameras[target].gameObject);
            }
            _entityCameras.Remove(target);
        }
    }

    void HandleShooterAdded(Shooter shooter)
    {
        shooter.OnTargetSelected += HandleTargetSelected;
        shooter.OnTargetingEnd += HandleTargetingEnd;
    }

    void HandleShooterRemoved(Shooter shooter)
    {
        shooter.OnTargetSelected -= HandleTargetSelected;
        shooter.OnTargetingEnd -= HandleTargetingEnd;
    }

    void HandleCharacter_OnActiveChanged(Character character, bool active)
    {
        if (active)
        {
            // disable any active camera
            _actionCamera.gameObject.SetActive(false);
            _activeEntityCamera.gameObject.SetActive(false);
            //
            _activeEntityCamera = _entityCameras[character.GetComponent<GridEntity>()];
            _activeEntityCamera.gameObject.SetActive(true);
            _lastActivatedCharacterCamera = _activeEntityCamera;
            _aimingCamera.m_Follow = character.transform;
        }
    }

    void HandleTargetSelected(Shooter shooter, GridEntity target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _aimingCamera.gameObject.SetActive(true);
        OnAimingCameraActiveChanged(true);
        _aimingCamera.LookAt = target.transform;
    }

    void HandleTargetingEnd()
    {
        _aimingCamera.gameObject.SetActive(false);
        OnAimingCameraActiveChanged(false);
        _activeEntityCamera = _lastActivatedCharacterCamera;
        _lastActivatedCharacterCamera.gameObject.SetActive(true);
    }

    void HandleMouseOverTarget(ShotStats target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _activeEntityCamera = _entityCameras[target.Target];
        _activeEntityCamera.gameObject.SetActive(true);
    }

    void HandleMouseExitTarget(ShotStats target)
    {
        _activeEntityCamera.gameObject.SetActive(false);
        _activeEntityCamera = _lastActivatedCharacterCamera;
        _lastActivatedCharacterCamera.gameObject.SetActive(true);
    }

    
}
