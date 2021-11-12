using UnityEngine;
using System.Collections;
using Mirror;

public class Hunkerer : BattleAction
{
    GridEntity _gridEntity;
    GridAgent _gridAgent;

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
    }

    public override void Init(int numActions)
    {
        base.Init(numActions);
        bool canHunker = false;
        for (int i = 0; i < 4; i++)
        {
            canHunker |= _gridEntity.CurrentNode.HalfWalls[i];
            canHunker |= _gridEntity.CurrentNode.Walls[i];
        }
        Available = canHunker;
        _gridAgent.Hunkering = false;
    }

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
        Hunker();
    }

    void Hunker()
    {
        CmdHunker();
    }

    [Command]
    void CmdHunker()
    {
        RpcHunker();
    }

    [ClientRpc]
    void RpcHunker()
    {
        Debug.Log("Hunkerer RpcHunker");
        _gridAgent.Hunkering = true;
        InvokeActionConfirmed(this);
        Deactivate();
        InvokeActionComplete(this);
    }
}
