using UnityEngine;
using Mirror;

public class Skipper : BattleAction
{
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
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }
}