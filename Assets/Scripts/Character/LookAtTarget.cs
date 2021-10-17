using UnityEngine;
using System.Collections;

public class LookAtTarget : MonoBehaviour
{
    Vector3 _direction;
    // rotation
    float _turnSmoothVelocity;
    float _turnSmoothTime = .1f;

    private void Awake()
    {
        Shooter[] shooters = GetComponents<Shooter>();
        for (int i = 0; i < shooters.Length; i++)
        {
            shooters[i].OnTargetSelected += HandleShooter_TargetSelected;
        }
        Overwatcher[] overwatchers = GetComponents<Overwatcher>();
        for (int i = 0; i < overwatchers.Length; i++)
        {
            overwatchers[i].OnShoot += HandleShooter_Shoot;
        }
        Thrower[] throwers = GetComponents<Thrower>();
        for (int i = 0; i < throwers.Length; i++)
        {
            throwers[i].OnTargetSelected += HandleThrower_TargetSelected;
        }
    }

    void HandleShooter_TargetSelected(Shooter arg1, GridEntity arg2)
    {
        _direction = arg2.transform.position - arg1.transform.position;
        _direction.y = 0;
    }

    void HandleShooter_Shoot(Shooter arg1, Character arg2)
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
