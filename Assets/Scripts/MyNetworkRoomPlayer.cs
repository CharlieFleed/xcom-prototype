using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkRoomPlayer : NetworkRoomPlayer
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("MyNetworkRoomPlayer OnStartClient");
    }
}
