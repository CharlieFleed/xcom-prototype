using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageHelper
{
    public static void DealDamage(Health health, Armor armor, Damage damage, int hitChance, int critChance, out bool hit, out bool crit)
    {
        int hitRandom = NetworkRandomGenerator.Instance.RandomRange(0, 100);
        int critRandom = NetworkRandomGenerator.Instance.RandomRange(0, 100);
        hit = hitRandom < hitChance ? true : false;
        crit = critRandom < critChance ? true : false;
        int healthDamage = 0;
        int armorDamage = 0;
        // process normal damage
        if (damage.MaxDamage > 0)
        {
            //Debug.Log($"normal damage");
            int dmg = 0;
            if (hit)
            {
                if (crit)
                {
                    dmg = NetworkRandomGenerator.Instance.RandomRange(damage.MaxDamage + 1, damage.MaxDamage + 1 + damage.BaseDamage);
                }
                else
                {
                    dmg = NetworkRandomGenerator.Instance.RandomRange(damage.BaseDamage, damage.MaxDamage + 1);
                }
            }
            //Debug.Log($"dmg = {dmg}");
            if (armor)
            {
                healthDamage = Mathf.Clamp(dmg - armor.Value, 0, int.MaxValue);
            }
            else
            {
                healthDamage = Mathf.Clamp(dmg, 0, int.MaxValue);
            }
            health.TakeDamage(healthDamage, hit, crit);
        }
        // process healing
        else if (damage.MaxDamage < 0)
        {
            int dmg = 0;
            if (hit)
            {
                if (crit)
                {
                    dmg = NetworkRandomGenerator.Instance.RandomRange(Mathf.Abs(damage.MaxDamage) + 1, Mathf.Abs(damage.MaxDamage) + 1 + Mathf.Abs(damage.BaseDamage));
                }
                else
                {
                    dmg = NetworkRandomGenerator.Instance.RandomRange(Mathf.Abs(damage.BaseDamage), Mathf.Abs(damage.MaxDamage) + 1);
                }
            }
            healthDamage = Mathf.Clamp(-dmg, int.MinValue, 0);
            health.TakeDamage(healthDamage, hit, crit);
        }
        // process explosive damage
        else if (damage.MaxExplosiveDamage > 0)
        {
            //Debug.Log($"explosive damage");
            int dmg = 0;
            if (hit)
            {
                if (crit)
                {
                    dmg = NetworkRandomGenerator.Instance.RandomRange(damage.MaxExplosiveDamage + 1, damage.MaxExplosiveDamage + 1 + damage.BaseExplosiveDamage);
                }
                else
                {
                    dmg = NetworkRandomGenerator.Instance.RandomRange(damage.BaseExplosiveDamage, damage.MaxExplosiveDamage + 1);
                }
            }
            //Debug.Log($"dmg = {dmg}");
            if (armor)
            {
                while (dmg > 1 && armorDamage < armor.Value)
                {
                    armorDamage++;
                    dmg -= 2;
                }
                armor.TakeDamage(armorDamage, hit, crit);
                healthDamage = Mathf.Clamp(dmg - armor.Value, 0, int.MaxValue); // if 1 dmg and armor left, health damage is zero
                health.TakeDamage(healthDamage, hit, crit);
            }
            else
            {
                healthDamage = Mathf.Clamp(dmg, 0, int.MaxValue);
                health.TakeDamage(healthDamage, hit, crit);
            }
        }        
    }    
}
