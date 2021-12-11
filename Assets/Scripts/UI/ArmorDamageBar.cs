using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmorDamageBar : MonoBehaviour
{
    #region Fields

    [SerializeField] Image _bg;
    [SerializeField] Image _critBg;
    [SerializeField] Image _missBg;
    [SerializeField] Image _healBg;
    [SerializeField] Text _text;
    [SerializeField] float _positionOffset = 3;
    [SerializeField] CanvasGroup _cg;
    [SerializeField] GameObject _pivot;

    public Armor Armor { get; private set; }
    Camera _camera;

    public event Action<ArmorDamageBar> OnEnd = delegate { };

    #endregion

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void SetArmor(Armor armor)
    {
        Armor = armor;
    }

    public void SetOffset(int offset)
    {
        _pivot.transform.Translate(Vector3.up * offset * 20);
    }

    public void SetDamage(int damage, bool hit, bool crit)
    {
        _text.text = hit ? (Mathf.Abs(damage).ToString() + (crit ? " ARMOR" : (damage >= 0 ? " ARMOR" : " HEAL"))) : "MISS";
        _bg.enabled = hit && !crit && damage > 0;
        _healBg.enabled = damage < 0;
        _critBg.enabled = crit;
        _missBg.enabled = !hit || damage == 0;
        _cg.alpha = 1;
        StartCoroutine(Hide());
    }

    private void LateUpdate()
    {
        transform.position = _camera.WorldToScreenPoint(Armor.transform.position + Vector3.up * _positionOffset);
    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(2);
        _cg.alpha = 0;
        OnEnd(this);
    }
}
