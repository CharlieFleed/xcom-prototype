using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] int _healthDefault;
    static int _staticHealthId = 0;

    public static event Action<Health> OnHealthAdded = delegate { };
    public static event Action<Health> OnHealthRemoved = delegate { };

    // Used by HealthBar
    public event Action<int, int> OnHealthChanged = delegate { };
    // Used by DamageBarController
    public event Action<Health, int, bool, bool> OnTakeDamage = delegate { };

    public event Action OnDied = delegate { };

    int _maxHealth;
    int _currentHealth;

    public int Id;

    private void Awake()
    {
        Id = _staticHealthId;
        _staticHealthId++;
    }

    private void OnDestroy()
    {
        OnHealthRemoved(this);
    }

    private void Start()
    {
        if (_healthDefault > 0)
        {
            _maxHealth = _healthDefault;
            _currentHealth = _maxHealth;
        }
        else
        {
            _maxHealth = UnityEngine.Random.Range(1, 11);
            _currentHealth = _maxHealth;
        }
        OnHealthAdded(this);
        OnHealthChanged(_currentHealth, _maxHealth);
    }

    public void TakeDamage(int damage, bool hit, bool crit)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged(_currentHealth, _maxHealth);
        OnTakeDamage(this, damage, hit, crit);
        if (IsDead)
        {
            OnDied();
        }
    }

    public bool IsDead { get { return _currentHealth == 0; }}

    public bool IsLow { get { return _currentHealth < .3f * _maxHealth; } }

    public bool IsFull { get { return _currentHealth == _maxHealth; } }
}
