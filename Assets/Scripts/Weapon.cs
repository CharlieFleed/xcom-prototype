using UnityEngine;
using System.Collections;
using System;

public class Weapon : MonoBehaviour
{
    [SerializeField] string _name;
    [SerializeField] int _baseDamage = 3;
    [SerializeField] int _maxDamage = 5;
    [SerializeField] int _range = 10;
    [SerializeField] int _clipSize = 4;
    [SerializeField] GameObject _shootFXPrefab;
    
    public Sprite Image;

    int _bullets;

    public string Name { get { return _name; } }
    public int BaseDamage { get { return _baseDamage; } }
    public int MaxDamage { get { return _maxDamage; } }
    public int Range { get { return _range; } }
    public int ClipSize { get { return _clipSize; } }
    public int Bullets { get { return _bullets; } }

    // Used by ClipBar
    public event Action<int, int> OnAmmunitionsChanged = delegate { };

    private void Start()
    {
        Reload();
    }

    public void Reload()
    {
        _bullets = _clipSize;
        OnAmmunitionsChanged(_bullets, _clipSize);
    }

    public void Shoot()
    {
        // show shoot FX
        Instantiate(_shootFXPrefab, transform.position + Vector3.up + transform.forward, Quaternion.identity);
        _bullets--;
        OnAmmunitionsChanged(_bullets, _clipSize);
    }

    public int HitChanceBonus(GridEntity target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _range)
        {
            return -100;
        }
        else
        {
            return (int)(-30 * distance / _range);
        }
    }
}
