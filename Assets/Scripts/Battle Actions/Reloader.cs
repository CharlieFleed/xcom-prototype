using UnityEngine;
using System.Collections;
using Mirror;

public class Reloader : BattleAction
{
    [SerializeField] protected Weapon _weapon;

    public override string ActionName { get { return base.ActionName + " " + _weapon.Name; } }
    public override string ConfirmText { get { return base.ActionName + " " + _weapon.Name; } }

    private void Update()
    {
        if (IsActive)
        {
            _input.Update();
        }
    }

    private void LateUpdate() // NOTE: Late Update to avoid right click read by GridPathSelector as well
    {
        if (IsActive)
        {
            if (_input.GetKeyDown(KeyCode.Escape) || _input.GetMouseButtonDown(1))
            {
                Cancel();
            }
        }
        _input.Clear();
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
        CmdReload();
    }

    [Command]
    void CmdReload()
    {
        RpcReload();
    }

    [ClientRpc]
    void RpcReload()
    {
        Debug.Log("Reloader RpcReload");
        _weapon.Reload();
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }
}
