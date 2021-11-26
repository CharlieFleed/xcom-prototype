using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmorBar : MonoBehaviour
{
    [SerializeField] Image[] _armors;
    [SerializeField] Image[] _brokenArmors;

    Armor _armor;
    int _current = 0;
    float _animationTime = 1.5f;

    private void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            _brokenArmors[i].enabled = false;
        }
    }

    public void SetArmor(Armor armor)
    {
        _armor = armor;
        _armor.OnArmorChanged += HandleArmorChanged;
    }

    void HandleArmorChanged(int armor)
    {
        if (armor < _current)
        {
            StartCoroutine(AnimateBrokenArmor(armor, _current));
        }
        for (int i = 0; i < 10; i++)
        {
            _armors[i].enabled = armor > i;
        }
        _current = armor;
    }

    IEnumerator AnimateBrokenArmor(int start, int end)
    {
        Debug.Log($"start{start} end{end}");
        for (int i = start; i < end; i++)
        {
            _brokenArmors[i].enabled = true;
            _brokenArmors[i].CrossFadeAlpha(0, _animationTime, true);
        }
        yield return new WaitForSecondsRealtime(_animationTime);
        for (int i = start; i < end; i++)
        {
            _brokenArmors[i].enabled = false;
        }
    }
}
