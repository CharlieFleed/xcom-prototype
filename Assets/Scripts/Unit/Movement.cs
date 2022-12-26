using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] Animator _animator;
    [SerializeField] float _walkSpeed = 15;
    [SerializeField] float _climbSpeed = 2;
    [SerializeField] float _distanceToTop = 1.5f;
    [SerializeField] float _distanceToWall = 0.5f;

    Transform _transform;

    // movement
    bool _isMoving;

    bool _isWalking;

    bool _isLeaping;

    bool _isJumping;

    bool _isJumpingWalk;
    bool _isJumpingCharge;
    bool _isJumpingFall;
    bool _isJumpingLand;

    bool _isClimbing;

    bool _isClimbingTurn;
    bool _isClimbingApproach;
    bool _isClimbingClimb;
    bool _isClimbingToTop;
    bool _isClimbingRun;

    Vector3 _newPosition;
    Vector3 _prevPosition;

    float _t;
    Vector3 _originPosition;
    Vector3 _startPosition;
    Vector3 _targetPosition;
    float _timeToReachTarget;

    // jumping
    float _ySpeed = 0;
    float _g = 9.8f;

    // rotation
    float _turnSmoothVelocity;
    float _turnSmoothTime = .1f;
    Vector3 _lookAtDirection = Vector3.zero;

    public Vector3 TargetPosition { get { return _targetPosition; } }

    private void Awake()
    {
        //_animator = gameObject.GetComponentInChildren<Animator>();
        _transform = transform;
        _prevPosition = _transform.position;
    }

    private void Update()
    {
        _prevPosition = _transform.position;
        if (_isMoving)
        {
            if (_isClimbing)
            {
                UpdateClimbing();
            }
            if (_isJumping)
            {
                UpdateJumping();
            }
            if (_isWalking)
            {
                UpdateWalking();
            }
            if (_isLeaping)
            {
                UpdateLeaping();
            }
        }
        UpdateXZVelocity();
        UpdateRotation();
    }

    void ResetStates()
    {
        _isMoving = false;
        _isWalking = false;
        _isLeaping = false;
        _isJumping = false;
        _isJumpingWalk = false;
        _isJumpingCharge = false;
        _isJumpingFall = false;
        _isJumpingLand = false;
        _isClimbing = false;
        _isClimbingTurn = false;
        _isClimbingApproach = false;
        _isClimbingClimb = false;
        _isClimbingToTop = false;
        _isClimbingRun = false;
    }

    void UpdateWalking()
    {
        //Debug.Log("Update Walking");
        _t += Time.deltaTime / _timeToReachTarget;
        _newPosition = Vector3.Lerp(_startPosition, _targetPosition, _t);
        _transform.position = _newPosition;
        // check arrival to destination
        if (Mathf.Abs(_transform.position.x - _targetPosition.x) < 0.01f && Mathf.Abs(_transform.position.z - _targetPosition.z) < 0.01f)
        {
            //Debug.Log("Arrived to destination.");
            _transform.position = _targetPosition;
            ResetStates();
        }
    }

    void UpdateLeaping()
    {
        //Debug.Log("Update Leaping");
        _t += Time.deltaTime / _timeToReachTarget;
        _newPosition = Vector3.Lerp(_startPosition, _targetPosition, _t);
        _transform.position = _newPosition;
        // check arrival to destination
        if (Mathf.Abs(_transform.position.x - _targetPosition.x) < 0.01f && Mathf.Abs(_transform.position.z - _targetPosition.z) < 0.01f)
        {
            //Debug.Log("Arrived to destination.");
            _transform.position = _targetPosition;
            ResetStates();
        }
    }

    void UpdateJumping()
    {
        if (_isJumpingWalk)
        {
            //Debug.Log($"_t:{_t}, timeToReachTarget:{_timeToReachTarget}");
            _t += Time.deltaTime / _timeToReachTarget;
            _newPosition.x = Mathf.Lerp(_startPosition.x, _targetPosition.x, _t);
            _newPosition.y = _startPosition.y;
            _newPosition.z = Mathf.Lerp(_startPosition.z, _targetPosition.z, _t);
            if (_t >= 0.5f)
            {
                _isJumpingWalk = false;
                _newPosition.x = Mathf.Lerp(_startPosition.x, _targetPosition.x, 0.5f);
                _newPosition.y = _startPosition.y;
                _newPosition.z = Mathf.Lerp(_startPosition.z, _targetPosition.z, 0.5f);
                _startPosition = _newPosition;
                _animator.SetTrigger("JumpDown");
                _timeToReachTarget = Mathf.Sqrt(2 * (_startPosition.y - _targetPosition.y) / _g);
                _t = 0;
                _ySpeed = 0;
                _isJumpingFall = true;
            }
        }
        else if (_isJumpingCharge)
        {
            _newPosition = _startPosition;
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Jump Take Off"))
            {
                Debug.Log("Falling");
                _isJumpingCharge = false;
                _isJumpingFall = true;
            }
        }
        else if (_isJumpingFall)
        {
            _t += Time.deltaTime / _timeToReachTarget;
            _newPosition.x = Mathf.Lerp(_startPosition.x, _targetPosition.x, _t);
            _newPosition.z = Mathf.Lerp(_startPosition.z, _targetPosition.z, _t);
            _ySpeed += -_g * Time.deltaTime;
            _newPosition.y += _ySpeed * Time.deltaTime;
            // trigger landing animation
            if (_ySpeed < 0 && _newPosition.y > _targetPosition.y)
            {
                float timeToLand = -1 * (_newPosition.y - _targetPosition.y) / _ySpeed;
                //Debug.Log($"Y speed: {_ySpeed}; Height: {transform.position.y - _target.y}; TTL: {timeToLand}.");
                if (timeToLand < 0.15f)
                {
                    //Debug.Log("Landing");
                    _animator.SetTrigger("Land");
                    _isJumpingLand = true;
                    _isJumpingFall = false;
                }
            }
        }
        else if (_isJumpingLand)
        {
            _t += Time.deltaTime / _timeToReachTarget;
            _newPosition.x = Mathf.Lerp(_startPosition.x, _targetPosition.x, _t);
            _newPosition.z = Mathf.Lerp(_startPosition.z, _targetPosition.z, _t);
            _ySpeed += -_g * Time.deltaTime;
            _newPosition.y += _ySpeed * Time.deltaTime;
            // check arrival to destination
            if (Mathf.Abs(_newPosition.x - _targetPosition.x) < 0.01f && Mathf.Abs(_newPosition.z - _targetPosition.z) < 0.01f &&
                _ySpeed <= 0 &&
                _newPosition.y <= _targetPosition.y)
            {
                //Debug.Log("Arrived to destination.");
                _newPosition = _targetPosition;
                ResetStates();
                _ySpeed = 0;
            }
        }
        transform.position = _newPosition;
    }

    void UpdateClimbing()
    {
        Vector3 wallPosition = _startPosition + Vector3.ProjectOnPlane(_targetPosition - _startPosition, Vector3.up) * 0.5f - Vector3.ProjectOnPlane(_targetPosition - _startPosition, Vector3.up).normalized * _distanceToWall;
        if (_isClimbingTurn)
        {
            //Debug.Log($"_isClimbingTurn");
            if (_lookAtDirection == Vector3.zero)
            {
                //Debug.Log($"_isClimbingApproach true");
                _timeToReachTarget = Vector3.ProjectOnPlane(wallPosition - _startPosition, Vector3.up).magnitude / _walkSpeed;
                _isClimbingTurn = false;
                _isClimbingApproach = true;
            }
        }
        else if (_isClimbingApproach)
        {
            //Debug.Log($"_isClimbingApproach");
            _t += Time.deltaTime / _timeToReachTarget;
            _newPosition = Vector3.Lerp(_startPosition, wallPosition, _t);
            // check arrival to wall
            if (Mathf.Abs(_newPosition.x - wallPosition.x) < 0.01f && Mathf.Abs(_newPosition.z - wallPosition.z) < 0.01f)
            {
                //Debug.Log("Arrived to wall.");
                _newPosition = wallPosition;
                _startPosition = _newPosition;
                //Debug.Log($"_isClimbingClimb true");
                _timeToReachTarget = (_targetPosition.y - _startPosition.y - _distanceToTop) / _climbSpeed;
                _t = 0;
                _isClimbingApproach = false;
                _isClimbingClimb = true;
                _animator.SetTrigger("Climbing");
            }
        }
        else if (_isClimbingClimb)
        {
            //Debug.Log($"_isClimbingClimb");
            _t += Time.deltaTime / _timeToReachTarget;
            _newPosition.y = Mathf.Lerp(_startPosition.y, _targetPosition.y - _distanceToTop, _t);
            // check if reached the edge
            if (_newPosition.y >= _targetPosition.y - (_distanceToTop + 0.05f))
            {
                _isClimbingClimb = false;
                //Debug.Log($"_isClimbingToTop true");
                _newPosition.y = _targetPosition.y - _distanceToTop;
                _isClimbingToTop = true;
                _animator.SetTrigger("ClimbingToTop");
                //Debug.Log($"_animator.transform.position {_animator.transform.position.x},{_animator.transform.position.y},{_animator.transform.position.z}");
                _animator.applyRootMotion = true;
                //Debug.Log($"_animator.transform.position {_animator.transform.position.x},{_animator.transform.position.y},{_animator.transform.position.z}");
                _animator.transform.localPosition += 0.25f * Vector3.forward;
                //Debug.Log($"_animator.transform.position {_animator.transform.position.x},{_animator.transform.position.y},{_animator.transform.position.z}");
            }
        }
        else if (_isClimbingToTop)
        {
            //Debug.Log($"_isClimbingToTop");
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Move") && !_animator.GetAnimatorTransitionInfo(0).IsName("Move -> Climb"))
            {
                _animator.applyRootMotion = false;
                // align parent game object (player) with child game object (model)
                //Debug.Log($"transform.localPosition {transform.localPosition.x},{transform.localPosition.y},{transform.localPosition.z}");
                //Debug.Log($"_animator.transform.position {_animator.transform.position.x},{_animator.transform.position.y},{_animator.transform.position.z}");
                //Debug.Log($"_animator.transform.localPosition {_animator.transform.localPosition.x},{_animator.transform.localPosition.y},{_animator.transform.localPosition.z}");
                _newPosition = new Vector3(_animator.transform.position.x, _targetPosition.y, _animator.transform.position.z);
                _startPosition = _newPosition;
                //_animator.transform.localPosition = Vector3.zero;
                _timeToReachTarget = Vector3.ProjectOnPlane(_targetPosition - _startPosition, Vector3.up).magnitude / _walkSpeed;
                _t = 0;
                _isClimbingToTop = false;
                _isClimbingRun = true;
                Debug.Log($"_isClimbingRun true");
            }
        }
        else if (_isClimbingRun)
        {
            Debug.Log($"_isClimbingRun");
            _t += Time.deltaTime / _timeToReachTarget;
            _newPosition = Vector3.Lerp(_startPosition, _targetPosition, _t);
            // check arrival to destination
            if (Mathf.Abs(_newPosition.x - _targetPosition.x) < 0.01f && Mathf.Abs(_newPosition.z - _targetPosition.z) < 0.01f)
            {
                Debug.Log("Arrived to destination.");
                _newPosition = _targetPosition;
                ResetStates();
            }
        }
        _transform.position = _newPosition;
    }

    void UpdateXZVelocity()
    {
        Vector3 movement = _transform.position - _prevPosition;
        Vector3 velocity = movement / Time.deltaTime;
        float velocityX = Vector3.Dot(velocity.normalized, transform.right);
        float velocityZ = Vector3.Dot(velocity.normalized, transform.forward);
        _animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        _animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }

    void UpdateRotation()
    {
        Vector3 direction = _transform.position - _prevPosition;
        direction.y = 0;
        if (_lookAtDirection != Vector3.zero)
        {
            direction = _lookAtDirection;
        }
        else
        {
            direction = Vector3.ProjectOnPlane(_transform.position - _prevPosition, Vector3.up);
        }
        if (direction.magnitude > 0)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            _transform.rotation = Quaternion.Euler(0, angle, 0);
            if (Mathf.Abs(targetAngle - Mathf.Atan2(_transform.forward.x, _transform.forward.z) * Mathf.Rad2Deg) < 0.5f)
            {
                _transform.forward = direction.normalized;
            }
        }
        if (_transform.forward == _lookAtDirection)
        {
            _lookAtDirection = Vector3.zero;
        }
    }

    public void MoveToDestination(Vector3 destination, bool leap)
    {
        //Debug.Log($"MoveToDestination");
        //Debug.Log($"Move from {transform.position.x},{transform.position.y},{transform.position.z} to {destination.x},{destination.y},{destination.z}.");
        _targetPosition = destination;
        _startPosition = _transform.position;
        _originPosition = _transform.position;
        if (_targetPosition != _transform.position) // avoid situations where walk is triggered but update never called to reset it
        {
            //Debug.Log($"Move from {transform.position.x},{transform.position.y},{transform.position.z} to {destination.x},{destination.y},{destination.z}.");
            _isMoving = true;
            _t = 0;            

            // evaluate height difference
            // Climb
            if (destination.y > _transform.position.y + GetComponent<GridAgent>().MaxJumpUp)
            {
                _isClimbing = true;
                _lookAtDirection = Vector3.ProjectOnPlane(destination - _startPosition, Vector3.up).normalized;
                _isClimbingTurn = true;
                //Debug.Log("Climb");
            }
            // Jump Up
            else if (destination.y > _transform.position.y + 1)
            {
                _isJumping = true;
                _isJumpingCharge = true;
                _ySpeed = 1.5f * Mathf.Sqrt(0.5f * _g * (destination.y - _transform.position.y));
                //Debug.Log($"_ySpeed {_ySpeed}");
                _timeToReachTarget = (_ySpeed + Mathf.Sqrt(_ySpeed * _ySpeed + 2 * _g * (destination.y - _transform.position.y))) / _g;
                //Debug.Log($"_timeToReachTarget {_timeToReachTarget}");
                _ySpeed = 2.5f * Mathf.Sqrt(0.5f * _g * (destination.y - _transform.position.y));
                _animator.SetTrigger("Jump");
                //Debug.Log("Jump Up");
            }
            // Jump Down
            else if (destination.y < transform.position.y - 1)
            {
                _isJumping = true;
                _isJumpingWalk = true;                
                _timeToReachTarget = Vector3.ProjectOnPlane(destination - _startPosition, Vector3.up).magnitude / _walkSpeed;
                //Debug.Log("Jump Down");
            }
            // Leap
            else if (leap)
            {
                _isLeaping = true;
                _timeToReachTarget = 3.0f * Vector3.ProjectOnPlane(destination - _startPosition, Vector3.up).magnitude / _walkSpeed;
                _animator.SetTrigger("Leap");
                //Debug.Log("Leap");
            } 
            // Walk 
            else
            {
                _isWalking = true;
                _timeToReachTarget = Vector3.ProjectOnPlane(destination - _startPosition, Vector3.up).magnitude / _walkSpeed;
                //Debug.Log("Walk");
            }
        }
    }

    public bool IsAtDestination()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Move"))
        {
            return _transform.position == _targetPosition;
        }
        else
        {
            return false;
        }
    }

    public bool IsInMotion()
    {
        //Debug.Log($"{name} t: {_targetPosition.x},{_targetPosition.y},{_targetPosition.z}");
        //Debug.Log($"{name} a: {_transform.position.x},{_transform.position.y},{_transform.position.z}");
        //Debug.Log($"{name} b: {_originPosition.x},{_originPosition.y},{_originPosition.z}");
        //Debug.Log($"{name} d: {Vector3.Distance(_transform.position, _originPosition)}");
        return (_targetPosition != _originPosition) && (Vector3.Distance(_transform.position, _originPosition) > 0.5f * GridManager.Instance.XZScale);
    }
}
