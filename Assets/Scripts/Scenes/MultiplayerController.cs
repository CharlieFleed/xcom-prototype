using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerController : MonoBehaviour
{
    [SerializeField] string _parentScene;

    [SerializeField] MyNetworkRoomManager _networkManager;
    [SerializeField] InputField _ipAddress;
    [SerializeField] Button _joinButton;

    private void OnEnable()
    {
        //Debug.Log("Registering");
        MyNetworkRoomManager.OnClientDisconnected += MyNetworkRoomManager_OnClientDisconnected;
    }

    private void OnDisable()
    {
        //Debug.Log("Un-Registering");
        MyNetworkRoomManager.OnClientDisconnected -= MyNetworkRoomManager_OnClientDisconnected;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("LAST_ROOM"))
        {
            //Debug.Log("Loading LAST_ROOM from PlayerPrefs.");
            _ipAddress.text = PlayerPrefs.GetString("LAST_ROOM");
        }
    }

    /// <summary>
    /// This manages failed connections. On successful connection this object is destroyed before OnClientConnect is fired.
    /// </summary>
    private void MyNetworkRoomManager_OnClientDisconnected()
    {
        _joinButton.interactable = true;
    }

    public void Host()
    {
        _networkManager.StartHost();
    }

    public void Join()
    {
        string ipAddress = "";
        if (_ipAddress.text != "")
            ipAddress = _ipAddress.text;
        else
            ipAddress = _ipAddress.placeholder.GetComponent<Text>().text;
        _networkManager.networkAddress = ipAddress;
        _networkManager.StartClient();
        _joinButton.interactable = false;
    }

    public void BackButtonClick()
    {
        Back();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Back();
        }
    }

    private void Back()
    {
        if (NetworkClient.active)
        {
            _networkManager.StopClient();
        }
        Destroy(_networkManager.gameObject);
        SceneManager.LoadScene(_parentScene);
    }
}
