using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorDamageBarController : MonoBehaviour
{
    [SerializeField] ArmorDamageBar _damageBarPrefab;

    Dictionary<Armor, List<ArmorDamageBar>> _damageBars = new Dictionary<Armor, List<ArmorDamageBar>>();

    public void Awake()
    {
        Armor.OnArmorAdded += HandleArmorAdded;
        Armor.OnArmorRemoved += HandleArmorRemoved;
    }

    private void HandleArmorAdded(Armor armor)
    {
        if (_damageBars.ContainsKey(armor) == false)
        {
            _damageBars.Add(armor, new List<ArmorDamageBar>());
            armor.OnTakeDamage += HandleArmor_TakeDamage;
        }
    }

    void HandleArmorRemoved(Armor armor)
    {
        if (_damageBars.ContainsKey(armor) == true)
        {
            armor.OnTakeDamage -= HandleArmor_TakeDamage;
            List<ArmorDamageBar> damageBars = _damageBars[armor];
            _damageBars.Remove(armor);
            for (int i = 0; i < damageBars.Count; i++)
            {
                Destroy(damageBars[i]);
            }
        }
    }

    void HandleArmor_TakeDamage(Armor armor, int damage, bool hit, bool crit)
    {
        var damageBar = Instantiate(_damageBarPrefab, transform);
        damageBar.SetArmor(armor);
        damageBar.SetOffset(_damageBars[armor].Count);
        damageBar.SetDamage(damage, hit, crit);
        damageBar.OnEnd += HandleDamageBar_End;
        _damageBars[armor].Add(damageBar);
    }

    void HandleDamageBar_End(ArmorDamageBar damageBar)
    {
        _damageBars[damageBar.Armor].Remove(damageBar);
        Destroy(damageBar);
    }

    private void OnDestroy()
    {
        Armor.OnArmorAdded -= HandleArmorAdded;
        Armor.OnArmorRemoved -= HandleArmorRemoved;
    }
}
