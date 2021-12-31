using UnityEngine;
using System.Collections;

public class BattleEventUnitAction : BattleEvent
{
    NetworkMatchManager _networkMatchManager;

    public BattleEventUnitAction(NetworkMatchManager networkMatchManager)
    {
        _networkMatchManager = networkMatchManager;
        _networkMatchManager.OnActionComplete += _networkMatchManager_OnActionComplete;
    }

    private void _networkMatchManager_OnActionComplete()
    {
        _networkMatchManager.OnActionComplete -= _networkMatchManager_OnActionComplete;
        End();
    }
}
