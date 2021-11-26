using UnityEngine;
using System.Collections;

public abstract class Item : MonoBehaviour
{
    [SerializeField] protected int _uses = 2;

    public abstract string Name { get; }
    public abstract float Range { get; }
    public abstract int BaseDamage { get; }
    public abstract int MaxDamage { get; }

    public int Uses { get { return _uses; } }

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
