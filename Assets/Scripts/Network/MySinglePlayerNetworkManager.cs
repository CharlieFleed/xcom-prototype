using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySinglePlayerNetworkManager : NetworkManager
{
    [SerializeField] GameObject _networkRandomGeneratorPrefab;
    MyGamePlayer _player;

    public override void Awake()
    {
        base.Awake();
        //Debug.Log("MySinglePlayerNetworkManager Awake");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Debug.Log("MySinglePlayerNetworkManager OnServerAddPlayer");
        base.OnServerAddPlayer(conn);
        _player = conn.identity.GetComponent<MyGamePlayer>();
        for (int i = 0; i < 4; i++)
        {
            _player.MatchSettings.unitClasses[i] = i;
        }
        MatchBuilder.Instance.SinglePlayerMatchSetup(_player);
        NetworkMatchManager.Instance.RpcStartMatch();
        GameObject networkRandomGenerator = Instantiate(_networkRandomGeneratorPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(networkRandomGenerator);
        NetworkServer.SpawnObjects();
        GameController.Instance.IsSinglePlayer = true;
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        //Debug.Log("MySinglePlayerNetworkManager OnServerSceneChanged");
        base.OnServerSceneChanged(sceneName);
        //if (sceneName == onlineScene)
        //{
        //    NetworkMatchManager.Instance.SinglePlayerMatchSetup(_player);
        //    NetworkMatchManager.Instance.RpcStartMatch();
        //    GameObject networkRandomGenerator = Instantiate(_networkRandomGeneratorPrefab, Vector3.zero, Quaternion.identity);
        //    NetworkServer.Spawn(networkRandomGenerator);
        //    NetworkServer.SpawnObjects();
        //    GameController.Instance.IsSinglePlayer = true;
        //}
    }
}
