using UnityEngine;
using System.Collections;

public class Reloader : BattleAction
{
    [SerializeField] protected Weapon _weapon;

    public override string ActionName { get { return base.ActionName + " " + _weapon.Name; } }
    public override string ConfirmText { get { return base.ActionName + " " + _weapon.Name; } }

    private void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        Available &= _weapon.Bullets < _weapon.ClipSize;
    }

    override public void HandleConfirmClick()
    {
        Reload();
    }

    void Reload()
    {
        _weapon.Reload();
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }
}
