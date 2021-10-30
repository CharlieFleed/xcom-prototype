using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkRoomManager : NetworkRoomManager
{
    [SerializeField] GameObject _networkRandomGeneratorPrefab;
    public List<Player> GamePlayers { get; } = new List<Player>();

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);
        //Debug.Log($"OnRoomServerSceneChanged({sceneName})");
    }

    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
        //Debug.Log($"ServerChangeScene({newSceneName})");
    }

    public void GamePlayerStart(Player gamePlayer)
    {
        if (GamePlayers.Count == 2)
        {
            foreach (var gp in GamePlayers)
            {
                NetworkMatchManager.Instance.RpcRegisterPlayer(gp);
                NetworkMatchManager.Instance.InstantiateUnits(gp);
            }
            NetworkMatchManager.Instance.RpcStartMatch();
            GameObject networkRandomGenerator = Instantiate(_networkRandomGeneratorPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(networkRandomGenerator);
            NetworkServer.SpawnObjects();
        }
    }
}
