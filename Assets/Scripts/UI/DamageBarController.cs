using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBarController : MonoBehaviour
{
    [SerializeField] DamageBar _damageBarPrefab;

    Dictionary<Health, List<DamageBar>> _damageBars = new Dictionary<Health, List<DamageBar>>();

    public void Awake()
    {
        Health.OnHealthAdded += HandleHealthAdded;
        Health.OnHealthRemoved += HandleHealthRemoved;
    }

    private void HandleHealthAdded(Health health)
    {
        if (_damageBars.ContainsKey(health) == false)
        {
            _damageBars.Add(health, new List<DamageBar>());
            health.OnTakeDamage += HandleHealth_TakeDamage;
        }
    }

    void HandleHealthRemoved(Health health)
    {
        if (_damageBars.ContainsKey(health) == true)
        {
            health.OnTakeDamage -= HandleHealth_TakeDamage;
            List<DamageBar> damageBars = _damageBars[health];
            _damageBars.Remove(health);
            for (int i = 0; i < damageBars.Count; i++)
            {
                Destroy(damageBars[i].gameObject);
            }
        }
    }

    void HandleHealth_TakeDamage(Health health, int damage, bool hit, bool crit)
    {
        var damageBar = Instantiate(_damageBarPrefab, transform);
        damageBar.SetHealth(health);
        damageBar.SetOffset(_damageBars[health].Count);
        damageBar.SetDamage(damage, hit, crit);
        damageBar.OnEnd += HandleDamageBar_OnEnd;
        _damageBars[health].Add(damageBar);
        Debug.Log("damage bar created");
    }

    void HandleDamageBar_OnEnd(DamageBar damageBar)
    {
        _damageBars[damageBar.Health].Remove(damageBar);
        Destroy(damageBar.gameObject);
    }

    private void OnDestroy()
    {
        Health.OnHealthAdded -= HandleHealthAdded;
        Health.OnHealthRemoved -= HandleHealthRemoved;
    }
}
