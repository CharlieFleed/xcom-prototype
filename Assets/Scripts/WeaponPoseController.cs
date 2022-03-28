using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPoseController : MonoBehaviour
{
    Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        BattleEventShot.OnShooting += HandleBattleEventShot_OnShooting;
        BattleEventOverwatchShot.OnOverwatchShooting += HandleBattleEventShot_OnShooting;
    }

    private void OnDestroy()
    {
        BattleEventShot.OnShooting -= HandleBattleEventShot_OnShooting;
        BattleEventOverwatchShot.OnOverwatchShooting -= HandleBattleEventShot_OnShooting;
    }

    void HandleBattleEventShot_OnShooting(Shooter shooter, GridEntity target)
    {
        Debug.Log($"HandleBattleEventShot_OnShooting({shooter.name},{target.name})");
        // if it's me shooting
        if (shooter.gameObject == gameObject.transform.root.gameObject)
        {
            Aim(shooter.Weapon.WeaponType);
        }
    }

    public void Extract(WeaponType weaponType)
    {
        PutWeaponDown();
        switch (weaponType)
        {
            case WeaponType.Rifle:
                _animator.SetBool("Rifle Extract", true);
                break;
            case WeaponType.Gun:
                _animator.SetBool("Gun Extract", true);
                break;
        }
    }

    public void PutWeaponDown()
    {
        _animator.SetBool("Rifle Aim", false);
        _animator.SetBool("Rifle Extract", false);
        _animator.SetBool("Gun Aim", false);
        _animator.SetBool("Gun Extract", false);
    }

    public void Aim(WeaponType weaponType)
    {
        Debug.Log("Aim");
        PutWeaponDown();
        switch (weaponType)
        {
            case WeaponType.Rifle:
                _animator.SetBool("Rifle Extract", true);
                _animator.SetBool("Rifle Aim", true);
                break;
            case WeaponType.Gun:
                _animator.SetBool("Gun Extract", true);
                _animator.SetBool("Gun Aim", true);
                break;
        }
    }

    public void Shoot()
    {
        _animator.SetTrigger("Shoot");
    }
}
