using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGamePlayer : NetworkBehaviour
{
    #region Fields

    public MatchSettings MatchSettings = new MatchSettings() { unitClasses = new int[6] };

    public bool IsActive { set; get; }

    NetworkMatchManager _matchManager;
    GridAgent _gridAgent;
    UnitDecisionTree _unitDecisionTree;

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
        //Debug.Log("MyGamePlayer OnStartClient");
        if (NetworkRoomManager != null)
        {
            NetworkRoomManager.GamePlayers.Add(this);
            if (isServer)
                NetworkRoomManager.GamePlayerStart(this);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        //Debug.Log("MyGamePlayer OnStopClient");
        if (NetworkRoomManager != null)
            NetworkRoomManager.GamePlayers.Remove(this);
    }

    private void Start()
    {
        _matchManager = NetworkMatchManager.Instance;
    }

    public void Activate()
    {
        IsActive = true;
        Unit currentUnit = _matchManager.CurrentUnit;
        if (isLocalPlayer)
        {
            if (currentUnit.Team.IsAI)
            {
                StartCoroutine(ActivateAI());
            }
            else
            {
                currentUnit.GetComponent<UnitLocalController>().Activate();
            }
        }
    }

    IEnumerator ActivateAI()
    {
        yield return new WaitForSeconds(1.5f);
        Unit currentUnit = _matchManager.CurrentUnit;
        _unitDecisionTree = currentUnit.GetComponent<UnitDecisionTree>();
        _unitDecisionTree.Run();
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
