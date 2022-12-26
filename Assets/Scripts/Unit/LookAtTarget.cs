using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates the transform to look at the target selected by any shooter/overwatcher/thrower
/// </summary>
public class LookAtTarget : MonoBehaviour
{
    Vector3 _direction;
    // rotation
    float _turnSmoothVelocity;
    float _turnSmoothTime = .1f;

    Shooter[] _shooters;
    Overwatcher[] _overwatchers;
    Thrower[] _throwers;

    private void Awake()
    {
        _shooters = GetComponents<Shooter>();
        for (int i = 0; i < _shooters.Length; i++)
        {
            _shooters[i].OnTargetSelected += HandleShooter_OnTargetSelected;
        }
        _overwatchers = GetComponents<Overwatcher>();
        for (int i = 0; i < _overwatchers.Length; i++)
        {
            _overwatchers[i].OnShoot += HandleOverwatcher_Shoot;
        }
        _throwers = GetComponents<Thrower>();
        for (int i = 0; i < _throwers.Length; i++)
        {
            _throwers[i].OnTargetSelected += HandleThrower_TargetSelected;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _shooters.Length; i++)
        {
            _shooters[i].OnTargetSelected -= HandleShooter_OnTargetSelected;
        }
        for (int i = 0; i < _overwatchers.Length; i++)
        {
            _overwatchers[i].OnShoot -= HandleOverwatcher_Shoot;
        }
        for (int i = 0; i < _throwers.Length; i++)
        {
            _throwers[i].OnTargetSelected -= HandleThrower_TargetSelected;
        }
    }

    void HandleShooter_OnTargetSelected(Shooter arg1, GridEntity arg2)
    {
        _direction = arg2.transform.position - arg1.transform.position;
        _direction.y = 0;
    }

    void HandleOverwatcher_Shoot(Shooter arg1, GridEntity arg2)
    {
        _direction = arg2.transform.position - arg1.transform.position;
        _direction.y = 0;
    }

    void HandleThrower_TargetSelected(Thrower arg1, GridNode arg2)
    {
        _direction = arg2.FloorPosition - arg1.transform.position;
        _direction.y = 0;
    }

    private void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        if (_direction.magnitude > 0)
        {
            float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            if (Mathf.Abs(targetAngle - Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg) < 0.5f)
            {
                transform.forward = new Vector3(_direction.x, 0, _direction.z);
                _direction = Vector3.zero;
            }
        }
    }
}
