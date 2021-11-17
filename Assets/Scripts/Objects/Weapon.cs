using UnityEngine;
using System.Collections;
using System;

public class Weapon : MonoBehaviour
{
    [SerializeField] WeaponData _weaponData;

    int _bullets;
    public int Bullets { get { return _bullets; } }

    public string Name { get { return _weaponData.Name; } }
    public int BaseDamage { get { return _weaponData.BaseDamage; } }
    public int MaxDamage { get { return _weaponData.MaxDamage; } }
    public int Range { get { return _weaponData.Range; } }
    public int ClipSize { get { return _weaponData.ClipSize; } }
    public bool InfiniteAmmo { get { return _weaponData.InfiniteAmmo; } }
    public Sprite Image { get { return _weaponData.Image; } }
    public GameObject HitFXPrefab { get { return _weaponData.HitFXPrefab; } }

    // Used by WeaponPanel
    public event Action<int, int> OnAmmunitionsChanged = delegate { };

    private void Start()
    {
        Reload();
    }

    public void Reload()
    {
        _bullets = ClipSize;
        OnAmmunitionsChanged(_bullets, ClipSize);
    }

    public void Shoot()
    {
        // show shoot FX
        if (_weaponData.ShootFXPrefab)
            Instantiate(_weaponData.ShootFXPrefab, transform.position + Vector3.up + transform.forward, Quaternion.identity);
        if (_weaponData.ShootAudioClip)
            AudioSource.PlayClipAtPoint(_weaponData.ShootAudioClip, transform.position);
        if (!InfiniteAmmo)
        {
            _bullets--;
            OnAmmunitionsChanged(_bullets, ClipSize);
        }
    }

    public int HitChanceBonus(GridEntity target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > Range)
        {
            return -100;
        }
        else
        {
            return (int)(-30 * distance / Range);
        }
    }
}
