using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator _animator;
    Shooter[] _shooters;
    Thrower[] _throwers;
    Health _health;
    Hunkerer _hunkerer;


    bool _isShooting;
    Shooter _currentShooter;
    bool _isThrowing;
    Thrower _currentThrower;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _shooters = GetComponents<Shooter>();
        foreach (Shooter shooter in _shooters)
        {
            shooter.OnShoot += HandleShooter_OnShoot;
        }
        _throwers = GetComponents<Thrower>();
        foreach (Thrower thrower in _throwers)
        {
            thrower.OnThrow += HandleThrower_OnThrow;
        }
        //Debug.Log($"AnimationController found {_throwers.Length} throwers.");
        _health.OnDied += HandleHealth_OnDied;
        _hunkerer = GetComponent<Hunkerer>();
        if (_hunkerer != null)
        {
            _hunkerer.OnIsHunkeringChanged += HandleHunkerer_OnIsHunkeringChanged;
        }
    }

    private void OnDestroy()
    {
        foreach (Shooter shooter in _shooters)
        {
            shooter.OnShoot -= HandleShooter_OnShoot;
        }
        foreach (Thrower thrower in _throwers)
        {
            thrower.OnThrow -= HandleThrower_OnThrow;
        }
        _health.OnDied -= HandleHealth_OnDied;
        if (_hunkerer != null)
        {
            _hunkerer.OnIsHunkeringChanged -= HandleHunkerer_OnIsHunkeringChanged;
        }
    }

    private void HandleHunkerer_OnIsHunkeringChanged(bool obj)
    {
        _animator.SetBool("Ducking", obj);
    }

    private void HandleHealth_OnDied()
    {
        _animator.SetTrigger("Die");
    }

    void HandleShooter_OnShoot(Shooter shooter)
    {
        _currentShooter = shooter;
        _animator.SetTrigger("SingleShot");
        _isShooting = true;
    }

    void HandleThrower_OnThrow(Thrower thrower)
    {
        //Debug.Log("AnimationController.HandleThrower_OnThrow");
        _currentThrower = thrower;
        _animator.SetTrigger("Toss Grenade");
        _isThrowing = true;
    }

    /// <summary>
    /// Called by animation event.
    /// </summary>
    public void Thrown()
    {
        //Debug.Log("AnimationController.Thrown");
        _isThrowing = false;
        _currentThrower.Thrown();
    }

    /// <summary>
    /// Called by animation event.
    /// </summary>
    public void Shot()
    {
        _isShooting = false;
        _currentShooter.Shot();
    }

    private void Update()
    {
        //if (_isShooting && _animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).IsName("Run"))
        //{
        //    _isShooting = false;
        //    _currentShooter.Shot();
        //}
        //if (_isThrowing && _animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).IsName("Run"))
        //{
        //    _isThrowing = false;
        //    _currentThrower.Thrown();
        //}
    }
}
