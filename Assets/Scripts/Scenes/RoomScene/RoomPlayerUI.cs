using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerUI : MonoBehaviour
{
    [SerializeField] public Button ReadyButton;
    [SerializeField] public Button NotReadyButton;
    [SerializeField] public Button RemoveButton;
    [SerializeField] public GameObject ReadyPanel;
    [SerializeField] public GameObject[] UnitPanels;

    MyNetworkRoomPlayer _player;

    private void Start()
    {
        ReadyPanel.SetActive(false);
    }

    public void OnReadyClick()
    {
        if (NetworkClient.active && !_player.readyToBegin)
        {
            _player.CmdChangeReadyState(true);
        }
    }

    public void OnNotReadyClick()
    {
        if (NetworkClient.active && _player.readyToBegin)
        {
            _player.CmdChangeReadyState(false);
        }
    }

    public void OnRemoveClick()
    {
        _player.GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
    }

    public void SetPlayer(MyNetworkRoomPlayer player)
    {
        _player = player;
        if (!player.isLocalPlayer)
        {
            foreach (var panel in UnitPanels)
            {
                    panel.GetComponent<UnitPanel>().Buttons.SetActive(false);
            }
            ReadyButton.gameObject.SetActive(false);
            NotReadyButton.gameObject.SetActive(false);
        }
        if (!((_player.isServer && _player.index > 0) || _player.isServerOnly))
        {
            RemoveButton.gameObject.SetActive(false);
        }
        _player.OnReadyStateChanged += _player_OnReadyStateChanged;
    }

    private void _player_OnReadyStateChanged(bool obj)
    {
        //Debug.Log($"Ready state changed to {obj}");
        ReadyPanel.SetActive(obj);
        ReadyButton.interactable = !obj;
        NotReadyButton.interactable = obj;
    }

    private void OnDestroy()
    {
        //Debug.Log("Destroy RoomPlayerUI");
        _player.OnReadyStateChanged -= _player_OnReadyStateChanged;
    }
}
