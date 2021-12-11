using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetsPanel : MonoBehaviour
{
    [SerializeField] TargetButton _targetButtonPrefab;

    Dictionary<ShotStats, TargetButton> _targetButtons = new Dictionary<ShotStats, TargetButton>();

    public event Action<ShotStats> OnMouseOverTarget = delegate { };
    public event Action<ShotStats> OnMouseExitTarget = delegate { };
    public event Action<ShotStats> OnTargetClick = delegate { };

    ShotStats _mouseOverTarget;

    private void OnEnable()
    {
        UnitLocalController.OnUnitLocalControllerAdded += Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved += Handle_UnitLocalControllerRemoved;
        Shooter.OnShooterAdded += HandleShooterAdded;
        Shooter.OnShooterRemoved += HandleShooterRemoved;
        ItemUser.OnItemUserAdded += HandleItemUser_OnItemUserAdded;
        ItemUser.OnItemUserRemoved += HandleItemUser_OnItemUserRemoved;
    }

    private void OnDisable()
    {
        UnitLocalController.OnUnitLocalControllerAdded -= Handle_UnitLocalControllerAdded;
        UnitLocalController.OnUnitLocalControllerRemoved -= Handle_UnitLocalControllerRemoved;
        Shooter.OnShooterAdded -= HandleShooterAdded;
        Shooter.OnShooterRemoved -= HandleShooterRemoved;
        ItemUser.OnItemUserAdded -= HandleItemUser_OnItemUserAdded;
        ItemUser.OnItemUserRemoved -= HandleItemUser_OnItemUserRemoved;
    }

    void HandleHideTargets()
    {
        foreach (var item in _targetButtons)
        {
            Destroy(item.Value.gameObject);
        }
        _targetButtons.Clear();
        _mouseOverTarget = null;
    }

    void HandleTargetAdded(ShotStats target)
    {
        if (_targetButtons.ContainsKey(target) == false)
        {
            var targetButton = Instantiate(_targetButtonPrefab, transform);
            _targetButtons.Add(target, targetButton);
            targetButton.SetTarget(target);
            int buttonWidth = (int)targetButton.GetComponent<RectTransform>().sizeDelta.x;
            GetComponent<RectTransform>().sizeDelta = new Vector2(_targetButtons.Count * buttonWidth, buttonWidth);
            targetButton.transform.localPosition = new Vector3((_targetButtons.Count - 1) * 0.5f * buttonWidth - 0.5f * buttonWidth, 0.5f * buttonWidth, 0);
            targetButton.OnTargetClick += HandleTargetClick;
            targetButton.OnMouseOverTarget += HandleMouseOverTarget;
            targetButton.OnMouseExitTarget += HandleMouseExitTarget;
            //Debug.Log("added target button");
        }
    }

    private void HandleMouseExitTarget(ShotStats obj)
    {
        if (_mouseOverTarget != null)
        {    
            _mouseOverTarget = null;
        }
        else
        {
            OnMouseExitTarget(obj);
        }
    }

    private void HandleMouseOverTarget(ShotStats obj)
    {
        OnMouseOverTarget(obj);
    }

    private void HandleTargetClick(ShotStats obj)
    {
        //Debug.Log($"HandleTargetClick");
        OnTargetClick(obj);
    }

    private void Handle_UnitLocalControllerAdded(UnitLocalController obj)
    {
        OnTargetClick += obj.HandleTargetClick; // NOTE: reversed event dependency
        OnMouseOverTarget += obj.HandleMouseOverTarget; // NOTE: reversed event dependency
        OnMouseExitTarget += obj.HandleMouseExitTarget; // NOTE: reversed event dependency
    }

    private void Handle_UnitLocalControllerRemoved(UnitLocalController obj)
    {
        OnTargetClick -= obj.HandleTargetClick;
        OnMouseOverTarget -= obj.HandleMouseOverTarget;
        OnMouseExitTarget -= obj.HandleMouseExitTarget;
    }

    private void HandleShooterAdded(Shooter shooter)
    {
        shooter.OnTargetAdded += HandleTargetAdded;
        shooter.OnHideTargets += HandleHideTargets;
        OnTargetClick += shooter.HandleTargetClick;  // NOTE: reversed event dependency
        OnMouseOverTarget += shooter.HandleMouseOverTarget;  // NOTE: reversed event dependency
        OnMouseExitTarget += shooter.HandleMouseExitTarget;  // NOTE: reversed event dependency
    }

    private void HandleShooterRemoved(Shooter shooter)
    {
        shooter.OnTargetAdded -= HandleTargetAdded;
        shooter.OnHideTargets -= HandleHideTargets;
        OnTargetClick -= shooter.HandleTargetClick;
        OnMouseOverTarget -= shooter.HandleMouseOverTarget;
        OnMouseExitTarget -= shooter.HandleMouseExitTarget;
    }

    private void HandleItemUser_OnItemUserAdded(ItemUser itemUser)
    {
        itemUser.OnTargetAdded += HandleTargetAdded;
        itemUser.OnHideTargets += HandleHideTargets;
        OnTargetClick += itemUser.HandleTargetClick;  // NOTE: reversed event dependency
        OnMouseOverTarget += itemUser.HandleMouseOverTarget;  // NOTE: reversed event dependency
        OnMouseExitTarget += itemUser.HandleMouseExitTarget;  // NOTE: reversed event dependency
    }

    private void HandleItemUser_OnItemUserRemoved(ItemUser itemUser)
    {
        itemUser.OnTargetAdded -= HandleTargetAdded;
        itemUser.OnHideTargets -= HandleHideTargets;
        OnTargetClick -= itemUser.HandleTargetClick;
        OnMouseOverTarget -= itemUser.HandleMouseOverTarget;
        OnMouseExitTarget -= itemUser.HandleMouseExitTarget;
    }

}
