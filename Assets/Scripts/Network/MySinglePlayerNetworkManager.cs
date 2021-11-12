using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySinglePlayerNetworkManager : NetworkManager
{
    [SerializeField] GameObject _networkRandomGeneratorPrefab;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        MyGamePlayer player = conn.identity.GetComponent<MyGamePlayer>();
        for (int i = 0; i < 4; i++)
        {
            player.MatchSettings.unitClasses[i] = i;
        }
        NetworkMatchManager.Instance.SinglePlayerMatchSetup(player);
        NetworkMatchManager.Instance.RpcStartMatch();
        GameObject networkRandomGenerator = Instantiate(_networkRandomGeneratorPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(networkRandomGenerator);
        NetworkServer.SpawnObjects();
        GameController.Instance.IsSinglePlayer = true;
    }
}
