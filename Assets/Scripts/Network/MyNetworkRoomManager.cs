using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MyNetworkRoomManager : NetworkRoomManager
{
    public static event Action OnClientConnected = delegate { };
    public static event Action OnClientDisconnected = delegate { };

    [SerializeField] GameObject _networkRandomGeneratorPrefab;
    public List<MyGamePlayer> GamePlayers { get; } = new List<MyGamePlayer>();

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);
        Debug.Log($"OnRoomServerSceneChanged({sceneName})");
    }

    public override void OnRoomClientSceneChanged(NetworkConnection conn)
    {
        base.OnRoomClientSceneChanged(conn);
        Debug.Log($"OnRoomClientSceneChanged()");
        if (IsSceneActive(RoomScene))
        {
            AddPlayerGUIs();
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
        Debug.Log($"ServerChangeScene({newSceneName})");
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        gamePlayer.GetComponent<MyGamePlayer>().MatchSettings = roomPlayer.GetComponent<MyNetworkRoomPlayer>().MatchSettings;
        Debug.Log($"OnRoomServerSceneLoadedForPlayer. About to destroy room player.");
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    public override void OnRoomClientEnter()
    {
        base.OnRoomClientEnter();
        Debug.Log("OnRoomClientEnter");
    }

    void AddPlayerGUIs()
    {
        foreach (NetworkRoomPlayer roomPlayer in roomSlots)
        {
            if (roomPlayer == null)
                continue;
            RoomController.Instance.AddPlayerGUI(roomPlayer.GetComponent<MyNetworkRoomPlayer>());
        }
    }

    /// <summary>
    /// Find the local player and broadcast its match settings. 
    /// </summary>
    void ApplyMatchSettings()
    {
        foreach (NetworkRoomPlayer roomPlayer in roomSlots)
        {
            if (roomPlayer == null)
                continue;
            if (roomPlayer.isLocalPlayer)
            {
                roomPlayer.GetComponent<MyNetworkRoomPlayer>().ApplyMatchSettings();
            }
        }
    }

    [Server]
    public void GamePlayerStart(MyGamePlayer gamePlayer)
    {        
        if (GamePlayers.Count == roomSlots.Count)
        {
            foreach (var gp in GamePlayers)
            {
                NetworkMatchManager.Instance.RpcRegisterPlayer(gp);
            }
            StartMatch();
        }
    }

    [Server]
    void StartMatch()
    {
        Debug.Log("StartMatch");
        NetworkMatchManager.Instance.RpcStartMatch();
        GameObject networkRandomGenerator = Instantiate(_networkRandomGeneratorPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(networkRandomGenerator);
        NetworkServer.SpawnObjects();

        GameController.Instance.IsSinglePlayer = false;
    }

    public void RoomPlayerStart(MyNetworkRoomPlayer player)
    {
        Debug.Log("RoomPlayerStart");
        RoomController.Instance.AddPlayerGUI(player);
        ApplyMatchSettings();
    }

    public void RoomPlayerStop(MyNetworkRoomPlayer player)
    {
        Debug.Log("RoomPlayerStop");
        RoomController.Instance.RemovePlayerGUI(player);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("OnClientDisconnect");
        OnClientDisconnected();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("OnClientConnect");
        OnClientConnected();
        //Debug.Log("Saving LAST_ROOM in PlayerPrefs.");
        PlayerPrefs.SetString("LAST_ROOM", networkAddress);
    }
}
