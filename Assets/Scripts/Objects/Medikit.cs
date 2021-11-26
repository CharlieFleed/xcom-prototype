using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medikit : Item
{
    [SerializeField] MedikitData _data;

    public override string Name { get { return _data.Name; } }
    public override float Range { get { return _data.Range; } }
    public override int BaseDamage { get { return _data.BaseDamage; } }
    public override int MaxDamage { get { return _data.MaxDamage; } }

    public GameObject EffectFXPrefab { get { return _data.EffectFXPrefab; } }


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
        health.TakeDamage(BaseDamage, true, false); // TODO: convert to use DamageDealer
        NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false);
        // show FX
        Instantiate(EffectFXPrefab, target.transform.position, EffectFXPrefab.transform.rotation);
    }
}
