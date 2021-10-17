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

    CanvasGroup _cg;

    Weapon _weapon;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        MatchManager.Instance.OnCharacterActivated += HandleMatchManager_CharacterActivated;
    }

    private void HandleMatchManager_CharacterActivated(Character character)
    {
        if (_weapon != null)
        {
            _weapon.OnAmmunitionsChanged -= HandleWeapon_AmmunitionsChanged;
        }
        _weapon = character.Weapon;
        _weapon.OnAmmunitionsChanged += HandleWeapon_AmmunitionsChanged;
        _image.sprite = _weapon.Image;
        SetAmmunitions(_weapon.Bullets, _weapon.ClipSize);
    }

    // Update is called once per frame
    void Update()
    {
        _cg.alpha = (MatchManager.Instance.ActiveCharacter != null) ? 1 : 0;
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
