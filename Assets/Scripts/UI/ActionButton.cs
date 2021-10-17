using System;
using UnityEngine;
using UnityEngine.UI;

internal class ActionButton : MonoBehaviour
{
    public event Action<BattleAction> OnClick;
    CanvasGroup _captionCG;
    Text _caption;
    BattleAction _battleAction;

    private void Awake()
    {
        _caption = gameObject.GetComponentInChildren<Text>();
        _captionCG = gameObject.GetComponentInChildren<CanvasGroup>();
    }

    public void SetAction(BattleAction battleAction)
    {
        _battleAction = battleAction;
        _caption.text = _battleAction.ActionName;
        if (!_battleAction.Available)
        {
            GetComponent<Image>().color = Color.gray;
        }
        if (battleAction.Icon != null)
        {
            GetComponent<Image>().sprite = battleAction.Icon;
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