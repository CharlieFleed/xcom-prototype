using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageBar : MonoBehaviour
{
    #region Fields

    [SerializeField] Image _bg;
    [SerializeField] Image _critBg;
    [SerializeField] Image _missBg;
    [SerializeField] Text _text;
    [SerializeField] float _positionOffset = 2;
    [SerializeField] CanvasGroup _cg;
    [SerializeField] GameObject _pivot;

    public Health Health { get; private set; }
    Camera _camera;

    public event Action<DamageBar> OnEnd = delegate { };
    
    #endregion

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void SetHealth(Health health)
    {
        Health = health;
    }

    public void SetOffset(int offset)
    {
        _pivot.transform.Translate(Vector3.up * offset * 20);
    }

    public void SetDamage(int damage, bool hit, bool crit)
    {
        _text.text = hit ? (damage.ToString() + (crit ? " CRITICAL" : " DAMAGE")) : "MISS";
        _bg.enabled = hit && !crit;
        _critBg.enabled = crit;
        _missBg.enabled = !hit;
        enabled = true;
        _cg.alpha = 1;
        StartCoroutine(Hide());
    }

    private void LateUpdate()
    {
        transform.position = _camera.WorldToScreenPoint(Health.transform.position + Vector3.up * _positionOffset);
    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(2);
        _cg.alpha = 0;
        OnEnd(this);
    }
}
