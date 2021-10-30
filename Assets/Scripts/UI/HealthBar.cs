using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] RawImage _segments;
    [SerializeField] Image _fill;

    Health _health;

    bool _init = false;

    private void Update()
    {
        if (!_init)
        {
            Character character = _health.GetComponent<Character>();
            if (character)
            {
                if (character.Team != null)
                {
                    _fill.color = NetworkMatchManager.Instance.TeamColors[int.Parse(character.Team.Name)];
                    _init = true;
                }
                else
                {
                    Debug.Log($"No team for health bar of {character.name}");
                }
            }
            else
            {
                _init = true;
            }
        }
    }

    void HandleHealthChanged(int health, int maxHealth)
    {
        _slider.maxValue = maxHealth;
        _slider.value = health;
        _segments.uvRect = new Rect(0, 0, maxHealth, 1);
    }

    public void SetHealth(Health health)
    {
        _health = health;
        _health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        _health.OnHealthChanged -= HandleHealthChanged;
    }
}
