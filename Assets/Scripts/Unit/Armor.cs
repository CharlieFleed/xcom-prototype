using UnityEngine;
using System.Collections;
using System;

public class Armor : MonoBehaviour
{
    [SerializeField] int _armorDefault;
    bool _initialized;

    public static event Action<Armor> OnArmorAdded = delegate { };
    public static event Action<Armor> OnArmorRemoved = delegate { };

    // Used by ArmorBar
    public event Action<int> OnArmorChanged = delegate { };

    int _value;
    public int Value { get { return _value; } }

    private void OnDestroy()
    {
        OnArmorRemoved(this);
    }

    private void Start()
    {
        _value = _armorDefault;
        OnArmorAdded(this);
        OnArmorChanged(_value);
    }

    public void TakeDamage(int damage)
    {
        _value -= damage;
        _value = Mathf.Clamp(_value, 0, int.MaxValue);
        OnArmorChanged(_value);
    }

    public void SetValue(int value)
    {
        _value = value;
        OnArmorChanged(_value);
    }

    void Initialize()
    {
        if (!_initialized && NetworkRandomGenerator.Instance.Ready())
        {
            OnArmorChanged(_value);
            _initialized = true;
        }
    }
}
