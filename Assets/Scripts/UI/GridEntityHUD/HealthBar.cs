using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] RawImage _segments;
    [SerializeField] Image _fill;
    [SerializeField] TMP_Text _text;

    [SerializeField] float _animationTime = 3f;

    Health _health;

    bool _initialized = false;    

    private void Update()
    {
        if (!_initialized)
        {
            Unit unit = _health.GetComponent<Unit>();
            if (unit != null)
            {
                if (unit.Team != null)
                {
                    _fill.color = NetworkMatchManager.Instance.TeamColors[int.Parse(unit.Team.Name)];
                    _initialized = true;
                }
                else
                {
                    Debug.Log($"No team for health bar of {unit.name}");
                }
            }
            else
            {
                _initialized = true;
            }
        }
    }

    void HandleHealthChanged(int health, int maxHealth)
    {
        _slider.maxValue = maxHealth;
        _segments.uvRect = new Rect(0, 0, maxHealth, 1);
        _text.text = health + "/" + maxHealth;
        StartCoroutine(AnimateHealth(health));
    }

    public void SetHealth(Health health)
    {
        _health = health;
        _health.OnHealthChanged += HandleHealthChanged;
    }

    IEnumerator AnimateHealth(int health)
    {
        float t = 0;
        while (t < _animationTime)
        {
            t += Time.deltaTime;
            _slider.value = Mathf.Lerp(_slider.value, health, t/_animationTime);
            yield return null;
        }
        _slider.value = health;
    }

    private void OnDestroy()
    {
        _health.OnHealthChanged -= HandleHealthChanged;
    }
}
