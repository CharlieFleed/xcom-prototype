using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] RawImage _segments;
    [SerializeField] Image _fill;
    [SerializeField] Image _image;
    [SerializeField] GameObject _clipBar;

    CanvasGroup _cg;

    Weapon _weapon;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0;
    }

    private void OnEnable()
    {
        UnitLocalController.OnActiveChanged += HandleUnit_OnActiveChanged;
    }

    private void OnDisable()
    {
        UnitLocalController.OnActiveChanged -= HandleUnit_OnActiveChanged;
    }

    private void HandleUnit_OnActiveChanged(UnitLocalController unit, bool active)
    {
        if (active)
        {
            if (_weapon != null)
            {
                _weapon.OnAmmunitionsChanged -= HandleWeapon_AmmunitionsChanged;
            }
            _weapon = unit.Weapon;
            if (_weapon != null)
            {
                _weapon.OnAmmunitionsChanged += HandleWeapon_AmmunitionsChanged;
                _image.sprite = _weapon.Image;
                if (_weapon.InfiniteAmmo)
                {
                    _clipBar.SetActive(false);
                }
                else
                {
                    _clipBar.SetActive(true);
                    SetAmmunitions(_weapon.Bullets, _weapon.ClipSize);
                }
                _cg.alpha = 1;
            }
            else
            {
                _cg.alpha = 0;
            }
        }
        else
        {
            _cg.alpha = 0;
        }
    }

    void HandleWeapon_AmmunitionsChanged(int bullets, int clipSize)
    {
        SetAmmunitions(bullets, clipSize);
    }

    void SetAmmunitions(int bullets, int clipSize)
    {
        _slider.maxValue = clipSize;
        _slider.value = bullets;
        _segments.uvRect = new Rect(0, 0, clipSize, 1);
    }
}
