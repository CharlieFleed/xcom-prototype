using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionDetailsPanel : MonoBehaviour
{
    [SerializeField] TMP_Text _hitChanceText;
    [SerializeField] TMP_Text _critChanceText;
    [SerializeField] TMP_Text _damageText;
    [SerializeField] TMP_Text _descriptionText;

    private void Awake()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
        Shooter.OnShotSelected += HandleShooter_OnShotSelected;
        Thrower.OnThrowing += HandleThrower_OnThrowing;
        //
        gameObject.SetActive(false);
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _damageText.GetComponent<CanvasGroup>().alpha = 0;
        _descriptionText.GetComponent<CanvasGroup>().alpha = 0;
    }

    private void OnDestroy()
    {
        // de-registration of events cannot be on OnDisable
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
        Shooter.OnShotSelected -= HandleShooter_OnShotSelected;
        Thrower.OnThrowing -= HandleThrower_OnThrowing;
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController unitLocalController)
    {
        //Debug.Log("ActionDetailsPanel HandleUnitAdded");
        unitLocalController.OnActionActivated += HandleActionActivated;
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController unitLocalController)
    {
        unitLocalController.OnActionActivated -= HandleActionActivated;
    }

    private void HandleShooter_OnShotSelected(ShotStats shot)
    {
        _hitChanceText.text = shot.HitChance + "%";
        _critChanceText.text = shot.CritChance + "%";
        _damageText.text = shot.BaseDamagePreview + "-" + shot.MaxDamagePreview; 
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 1;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 1;
        _damageText.GetComponent<CanvasGroup>().alpha = 1;
        _descriptionText.GetComponent<CanvasGroup>().alpha = 1;
    }

    private void HandleThrower_OnThrowing(Grenade grenade)
    {
        _hitChanceText.text = "100%";
        _critChanceText.text = "50%";
        _damageText.text = "1" + "-" + grenade.Damage;
        _descriptionText.text = grenade.Description;
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 1;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 1;
        _damageText.GetComponent<CanvasGroup>().alpha = 1;
        _descriptionText.GetComponent<CanvasGroup>().alpha = 1;
    }

    private void HandleActionActivated(BattleAction battleAction)
    {
        battleAction.OnActionConfirmed += HandleActionConfirmed;
        battleAction.OnActionCancelled += HandleActionCancelled;
        _descriptionText.text = battleAction.DescriptionText;
        _descriptionText.GetComponent<CanvasGroup>().alpha = 1;
        gameObject.SetActive(true);
    }

    void HandleActionConfirmed(BattleAction battleAction)
    {
        battleAction.OnActionConfirmed -= HandleActionConfirmed;
        battleAction.OnActionCancelled -= HandleActionCancelled;
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _damageText.GetComponent<CanvasGroup>().alpha = 0;
        _descriptionText.GetComponent<CanvasGroup>().alpha = 0;
        gameObject.SetActive(false);
    }

    void HandleActionCancelled(BattleAction battleAction)
    {
        battleAction.OnActionConfirmed -= HandleActionConfirmed;
        battleAction.OnActionCancelled -= HandleActionCancelled;
        _hitChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _critChanceText.GetComponent<CanvasGroup>().alpha = 0;
        _damageText.GetComponent<CanvasGroup>().alpha = 0;
        _descriptionText.GetComponent<CanvasGroup>().alpha = 0;
        gameObject.SetActive(false);
    }
}
