using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGamePlayer : NetworkBehaviour
{
    #region Fields

    public MatchSettings MatchSettings = new MatchSettings() { unitClasses = new int[4] };

    public bool IsActive { set; get; }

    NetworkMatchManager _matchManager;
    Pathfinder _pathfinder;
    GridManager _gridManager;
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
        _pathfinder = Pathfinder.Instance;
        _gridManager = GridManager.Instance;
    }

    public void Activate()
    {
        IsActive = true;
        if (isLocalPlayer)
        {
            if (_matchManager.CurrentTeamMember.Team.IsAI)
            {
                _unitDecisionTree = _matchManager.CurrentTeamMember.GetComponent<UnitDecisionTree>();
                _unitDecisionTree.Run();                
            }
            else
            {
                _matchManager.CurrentTeamMember.GetComponent<UnitLocalController>().Activate();
            }
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    bool IsNodeAvailable(GridNode node)
    {
        return _gridManager.IsNodeAvailable(node, _gridAgent);
    }
}
