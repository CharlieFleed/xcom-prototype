using UnityEngine;
using Mirror;

public class Skipper : BattleAction
{
    private void LateUpdate()
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

    override public void HandleConfirmClick()
    {
        Skip();
    }

    public void Skip()
    {
        CmdSkip();
    }

    [Command]
    void CmdSkip()
    {
        RpcSkip();
    }

    [ClientRpc]
    void RpcSkip()
    {
        Debug.Log($"{name} Skipper RpcSkip");
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }
}