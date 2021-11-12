using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetButton : MonoBehaviour
{
    public event Action<ShotStats> OnMouseOverTarget;
    public event Action<ShotStats> OnMouseExitTarget;
    public event Action<ShotStats> OnTargetClick;

    ShotStats _target;

    [SerializeField] TMP_Text _hitChanceText;
    [SerializeField] Image _targetImage;
    [SerializeField] Image _borderImage;

    bool _showHitChance;

    public void SetTarget(ShotStats target)
    {
        _target = target;
        _targetImage.sprite = target.Target.Icon;
        _hitChanceText.text = target.HitChance + "%";
        if (_target.Available)
        {
            if (_target.Friendly)
            {
                _targetImage.color = Color.cyan;
                _borderImage.color = Color.cyan;
                _hitChanceText.color = Color.cyan;
            }
            else if (_target.Flanked)
            {
                _targetImage.color = Color.yellow;
                _borderImage.color = Color.yellow;
                _hitChanceText.color = Color.yellow;
            }
            else if (_target.HalfCover)
            {
                //_image.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
                _targetImage.color = Color.red;
                _borderImage.color = Color.red;
                _hitChanceText.color = Color.red;
            }
            else
            {
                _targetImage.color = Color.red;
                _borderImage.color = Color.red;
                _hitChanceText.color = Color.red;
            }
        }
        if (!_target.Available)
        {
            _targetImage.color = Color.gray;
            _borderImage.color = Color.gray;
            _hitChanceText.color = Color.gray;
        }
    }
    public void OnMouseOver()
    {
        _showHitChance = true;
        OnMouseOverTarget(_target);
    }
    public void OnMouseExit()
    {
        _showHitChance = false;
        OnMouseExitTarget(_target);
    }
    public void OnClick()
    {
        OnTargetClick(_target);
    }

    private void Update()
    {
        _borderImage.enabled = _target.Target.IsTargeted;
        _hitChanceText.enabled = _target.Target.IsTargeted || _showHitChance;
    }
}
