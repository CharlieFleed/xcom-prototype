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

    private void Awake()
    {
        Character.OnCharacterAdded += HandleCharacterAdded;
        Character.OnCharacterRemoved += HandleCharacterRemoved;
        Shooter.OnShooterAdded += HandleShooterAdded;
        Shooter.OnShooterRemoved += HandleShooterRemoved;
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

    private void HandleCharacterAdded(Character character)
    {
        OnTargetClick += character.HandleTargetClick; // TODO: change this
        OnMouseOverTarget += character.HandleMouseOverTarget; // TODO: change this
        OnMouseExitTarget += character.HandleMouseExitTarget; // TODO: change this
    }

    private void HandleCharacterRemoved(Character character)
    {
        OnTargetClick -= character.HandleTargetClick;
        OnMouseOverTarget -= character.HandleMouseOverTarget;
        OnMouseExitTarget -= character.HandleMouseExitTarget;
    }

    private void HandleShooterAdded(Shooter shooter)
    {
        shooter.OnTargetAdded += HandleTargetAdded;
        shooter.OnHideTargets += HandleHideTargets;
        OnTargetClick += shooter.HandleTargetClick;  // TODO: change this
        OnMouseOverTarget += shooter.HandleMouseOverTarget;  // TODO: change this
        OnMouseExitTarget += shooter.HandleMouseExitTarget;  // TODO: change this
    }

    private void HandleShooterRemoved(Shooter shooter)
    {
        shooter.OnTargetAdded -= HandleTargetAdded;
        shooter.OnHideTargets -= HandleHideTargets;
        OnTargetClick -= shooter.HandleTargetClick;
        OnMouseOverTarget -= shooter.HandleMouseOverTarget;
        OnMouseExitTarget -= shooter.HandleMouseExitTarget;
    }

    private void OnDestroy()
    {
        Character.OnCharacterAdded -= HandleCharacterAdded;
        Character.OnCharacterRemoved -= HandleCharacterRemoved;
        Shooter.OnShooterAdded -= HandleShooterAdded;
        Shooter.OnShooterRemoved -= HandleShooterRemoved;
    }
}
