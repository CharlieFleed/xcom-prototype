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
            if (_matchManager.CurrentUnit.Team.IsAI)
            {
                StartCoroutine(ActivateAI());
            }
            else
            {
                _matchManager.CurrentUnit.GetComponent<UnitLocalController>().Activate();
            }
        }
    }

    IEnumerator ActivateAI()
    {
        yield return new WaitForSeconds(0.5f);
        _unitDecisionTree = _matchManager.CurrentUnit.GetComponent<UnitDecisionTree>();
        _unitDecisionTree.Run();
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
