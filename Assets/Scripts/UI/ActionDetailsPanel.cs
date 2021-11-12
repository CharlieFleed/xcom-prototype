using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionDetailsPanel : MonoBehaviour
{
    [SerializeField] TMP_Text _hitChanceText;
    [SerializeField] TMP_Text _critChanceText;
    [SerializeField] TMP_Text _damageText;

    private void Awake()
    {
        Unit.OnUnitAdded += HandleUnitAdded;
        Unit.OnUnitRemoved += HandleUnitRemoved;
        Shooter.OnShotSelected += HandleShooter_OnShotSelected;
        gameObject.SetActive(false);
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _damageText.GetComponent<CanvasGroup>().alpha = 0;
    }

    private void HandleShooter_OnShotSelected(ShotStats obj)
    {
        _hitChanceText.text = obj.HitChance + "%";
        _critChanceText.text = obj.CritChance + "%";
        _damageText.text = obj.BaseDamage + "-" + obj.MaxDamage; 
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 1;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 1;
        _damageText.GetComponent<CanvasGroup>().alpha = 1;
    }

    private void HandleUnitAdded(Unit unit)
    {
        unit.OnActionActivated += HandleActionActivated;
    }

    private void HandleUnitRemoved(Unit unit)
    {
        unit.OnActionActivated -= HandleActionActivated;
    }

    private void HandleActionActivated(BattleAction battleAction)
    {
        battleAction.OnActionConfirmed += HandleActionConfirmed;
        battleAction.OnActionCancelled += HandleActionCancelled;
        gameObject.SetActive(true);
    }

    void HandleActionConfirmed(BattleAction battleAction)
    {
        battleAction.OnActionConfirmed -= HandleActionConfirmed;
        battleAction.OnActionCancelled -= HandleActionCancelled;
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _damageText.GetComponent<CanvasGroup>().alpha = 0;
        gameObject.SetActive(false);
    }

    void HandleActionCancelled(BattleAction battleAction)
    {
        battleAction.OnActionConfirmed -= HandleActionConfirmed;
        battleAction.OnActionCancelled -= HandleActionCancelled;
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _damageText.GetComponent<CanvasGroup>().alpha = 0;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Unit.OnUnitAdded -= HandleUnitAdded;
        Unit.OnUnitRemoved -= HandleUnitRemoved;
        Shooter.OnShotSelected -= HandleShooter_OnShotSelected;
    }
}
