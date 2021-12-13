using UnityEngine;
using System.Collections;

public class BattleEventEndTurn : BattleEvent
{
    NetworkMatchManager _networkMatchManager;

    public BattleEventEndTurn(NetworkMatchManager networkMatchManager)
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
