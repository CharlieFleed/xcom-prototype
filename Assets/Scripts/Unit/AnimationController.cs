using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
    Animator _animator;
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
        _animator = GetComponentInChildren<Animator>();
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
        _health.OnDied += HandleHealth_OnDied;
        _hunkerer = GetComponent<Hunkerer>();
        if (_hunkerer != null)
        {
            _hunkerer.OnIsHunkeringChanged += HandleHunkerer_OnIsHunkeringChanged;
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
        _currentThrower = thrower;
        _animator.SetTrigger("Toss Grenade");
        _isThrowing = true;
    }

    public void Thrown()
    {
        _isThrowing = false;
        _currentThrower.Thrown();
    }

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
