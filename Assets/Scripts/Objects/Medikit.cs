using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medikit : Item
{
    [SerializeField] int _healment = 4;
    [SerializeField] GameObject _effectFXPrefab;

    public int Healment { get { return _healment; } set { _healment = value; } }

    public override bool IsApplicable(GridEntity target)
    {
        bool result = base.IsApplicable(target);
        Health health = target.GetComponent<Health>();
        if (health != null && !health.IsFull())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void UseOn(GridEntity target)
    {
        base.UseOn(target);
        Health health = target.GetComponent<Health>();
        health.TakeDamage(-Healment, true, false);
        NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false);
        // show FX
        Instantiate(_effectFXPrefab, target.transform.position, Quaternion.identity);
    }
}
