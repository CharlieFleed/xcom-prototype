using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public struct MatchSettings
{
    public int[] unitClasses;
}

public class MyNetworkRoomPlayer : NetworkRoomPlayer
{
    #region Fields

    public event Action<MyNetworkRoomPlayer, int, int> OnUnitClassChange = delegate { };
    public event Action<bool> OnReadyStateChanged = delegate { };

    public MatchSettings MatchSettings = new MatchSettings() { unitClasses = new int[4] };

    MyNetworkRoomManager _networkRoomManager;
    MyNetworkRoomManager NetworkRoomManager
    {
        get
        {
            if (_networkRoomManager != null) return _networkRoomManager;
            return _networkRoomManager = MyNetworkRoomManager.singleton as MyNetworkRoomManager;
        }
    }

    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkRoomManager.RoomPlayerStart(this);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        NetworkRoomManager.RoomPlayerStop(this);
    }

    public void SetUnitType(int unitPosition, int unitClass)
    {
        CmdSetUnitType(unitPosition, unitClass);
    }

    [Command]
    void CmdSetUnitType(int unitPosition, int unitClass)
    {
        RpcSetUnitType(unitPosition, unitClass);
    }

    [ClientRpc]
    void RpcSetUnitType(int unitPosition, int unitClass)
    {
        MatchSettings.unitClasses[unitPosition] = unitClass;
        OnUnitClassChange(this, unitPosition, unitClass);
    }

    public void ApplyMatchSettings()
    {
        for (int i = 0; i < MatchSettings.unitClasses.Length; i++)
        {
            SetUnitType(i, MatchSettings.unitClasses[i]);
        }
    }

    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();
        Debug.Log("MyNetworkRoomPlayer - OnClientEnterRoom");
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        OnReadyStateChanged(newReadyState);
    }
}
