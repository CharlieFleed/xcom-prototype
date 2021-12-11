using UnityEngine;
using System.Collections;

public class BattleEventTurn : BattleEvent
{
    NetworkMatchManager _networkMatchManager;

    public BattleEventTurn(NetworkMatchManager networkMatchManager)
    {
        _networkMatchManager = networkMatchManager;
    }

    public override void Run()
    {
        base.Run();
        if (_networkMatchManager.CurrentUnit == null)
        {
            End();
        }
    }
}
