using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarController : MonoBehaviour, IUIChildController
{
    [SerializeField] HealthBar _healthBarPrefab;
    [SerializeField] GridEntityHUDController _gridEntityHUDController;

    Dictionary<Health, HealthBar> _healthBars = new Dictionary<Health, HealthBar>();    

    public void Init()
    {
        Health.OnHealthAdded += Health_OnHealthAdded;
        Health.OnHealthRemoved += Health_OnHealthRemoved;
    }

    private void Health_OnHealthRemoved(Health health)
    {
        if (_healthBars.ContainsKey(health) == true)
        {
            if (_healthBars[health] != null)
            {
                Destroy(_healthBars[health].gameObject);
            }
            _healthBars.Remove(health);
        }
    }

    private void Health_OnHealthAdded(Health health)
    {
        //Debug.Log($"Health added for {health.name}.");
        if (_healthBars.ContainsKey(health) == false)
        {
            GridEntity gridEntity = health.GetComponent<GridEntity>();
            var healthBar = Instantiate(_healthBarPrefab, _gridEntityHUDController.GridEntityHUD(gridEntity).transform);
            _healthBars.Add(health, healthBar);
            healthBar.SetHealth(health);
        }
    }

    private void OnDestroy()
    {
        Health.OnHealthAdded -= Health_OnHealthAdded;
        Health.OnHealthRemoved -= Health_OnHealthRemoved;
    }
}
