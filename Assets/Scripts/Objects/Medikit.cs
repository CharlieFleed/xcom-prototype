using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medikit : Item
{
    [SerializeField] MedikitData _data;

    public override string Name { get { return _data.Name; } }
    public override float Range { get { return _data.Range; } }
    public Damage Damage { get { return _data.Damage; } }

    public GameObject EffectFXPrefab { get { return _data.EffectFXPrefab; } }


    public override bool IsApplicable(GridEntity target)
    {
        bool result = base.IsApplicable(target);
        Health health = target.GetComponent<Health>();
        if (health != null && !health.IsFull)
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
        Armor armor = target.GetComponent<Armor>();
        DamageDealer.DealDamage(health, armor, Damage, 100, 0, out bool hit, out bool crit);
        NetworkMatchManager.Instance.AddBattleEvent(new BattleEventDamage(), false, 0);
        // show FX
        Instantiate(EffectFXPrefab, target.transform.position, EffectFXPrefab.transform.rotation);
    }
}
