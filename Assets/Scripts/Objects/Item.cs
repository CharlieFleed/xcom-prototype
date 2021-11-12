using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    [SerializeField] string _name;
    [SerializeField] float _range = 1;
    [SerializeField] protected int _uses = 2;
    [SerializeField] int _baseDamage = 3;
    [SerializeField] int _maxDamage = 3;

    public string Name { get { return _name; } }
    public float Range { get { return _range; } set { _range = value; } }
    public int Uses { get { return _uses; } }
    public int BaseDamage { get { return _baseDamage; } }
    public int MaxDamage { get { return _maxDamage; } }

    public void Use()
    {
        _uses--;
    }

    public virtual bool IsApplicable(GridEntity target)
    {
        return true;
    }

    public virtual void UseOn(GridEntity target)
    {
        return;
    }
}
