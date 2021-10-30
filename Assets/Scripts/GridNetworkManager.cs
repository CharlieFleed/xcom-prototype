using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNetworkManager : NetworkManager
{
    List<GameObject> _players = new List<GameObject>();
    [SerializeField] GameObject _networkRandomGeneratorPrefab;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        //NetworkServer.AddPlayerForConnection(conn, player);
        //_players.Add(player);

        //if (_players.Count == 2)
        //{
        //    foreach (var p in _players)
        //    {
        //        NetworkMatchManager.Instance.RpcRegisterPlayer(p.GetComponent<Player>());
        //        NetworkMatchManager.Instance.InstantiateUnits(p.GetComponent<Player>());
        //    }
        //    NetworkMatchManager.Instance.RpcStartMatch();
        //    GameObject networkRandomGenerator = Instantiate(_networkRandomGeneratorPrefab, Vector3.zero, Quaternion.identity);
        //    NetworkServer.Spawn(networkRandomGenerator, player.gameObject);
        //    NetworkServer.SpawnObjects();
        //}
    }
}
