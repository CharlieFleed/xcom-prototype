using System;
using UnityEngine;
using UnityEngine.UI;

internal class ActionButton : MonoBehaviour
{
    public event Action<BattleAction> OnClick;
    [SerializeField] CanvasGroup _captionCG;
    [SerializeField] Text _caption;
    [SerializeField] CanvasGroup _usesCG;
    [SerializeField] Text _uses;
    BattleAction _battleAction;

    public void SetAction(BattleAction battleAction)
    {
        _battleAction = battleAction;
        _caption.text = _battleAction.ActionName;
        if (!_battleAction.Available)
        {
            GetComponent<Image>().color = Color.gray;
        }
        if (_battleAction.Icon != null)
        {
            GetComponent<Image>().sprite = _battleAction.Icon;
        }
        if (_battleAction is ItemUser)
        {
            _uses.text = "x" + ((ItemUser)_battleAction).Item.Uses.ToString();
            _usesCG.alpha = 1.0f;
        }
        else if (_battleAction is Thrower)
        {
            _uses.text = "x" + ((Thrower)_battleAction).Grenade.Uses.ToString();
            _usesCG.alpha = 1.0f;
        }
        else
        {
            _usesCG.alpha = 0.0f;
        }
    }

    void HandleClick()
    {
        OnClick(_battleAction);
    }

    public void OnMouseOver()
    {
        _captionCG.alpha = 1;
    }
    public void OnMouseExit()
    {
        _captionCG.alpha = 0;
    }
}