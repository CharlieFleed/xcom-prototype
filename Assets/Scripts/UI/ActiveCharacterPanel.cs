using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveCharacterPanel : MonoBehaviour
{
    [SerializeField] Text _nameText;
    [SerializeField] Text _classText;
    CanvasGroup _cg;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0;
        Unit.OnActiveChanged += HandleUnit_OnActiveChanged;
    }

    private void HandleUnit_OnActiveChanged(Unit unit, bool active)
    {
        if (active)
        {
            _nameText.text = unit.GetComponent<UnitDetails>().Name;
            _classText.text = unit.GetComponent<UnitDetails>().Class;
            _cg.alpha = 1;
        }
        else
        {
            _nameText.text = "";
            _classText.text = "";
            _cg.alpha = 0;
        }
    }

    private void OnDestroy()
    {
        Unit.OnActiveChanged -= HandleUnit_OnActiveChanged;
    }
}
