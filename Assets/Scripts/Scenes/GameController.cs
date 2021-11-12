using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameController : MonoBehaviour
{
    [SerializeField] NetworkMatchManager _networkMatchManager;
    [SerializeField] GameObject _pauseMenu;

    public bool IsSinglePlayer { set; get; }

    NetworkManager _networkManager;
    NetworkManager NetworkManager
    {
        get
        {
            if (_networkManager != null) return _networkManager;
            return _networkManager = NetworkManager.singleton as NetworkManager;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    public static bool GamePaused = false;

    private void OnEnable()
    {
        _networkMatchManager.OnPause += HandleNetworkMatchManager_OnPause;
    }

    private void OnDisable()
    {
        _networkMatchManager.OnPause -= HandleNetworkMatchManager_OnPause;
    }

    private void HandleNetworkMatchManager_OnPause()
    {
        Pause();
    }

    public void HandlePauseClick()
    {
        Pause();
    }

    void Pause()
    {
        _pauseMenu.SetActive(true);
        Time.timeScale = 0;
        GamePaused = true;
    }

    void CloseMenu()
    {
        _pauseMenu.SetActive(false);
        Time.timeScale = 1;
        GamePaused = false;
    }

    public void Resume()
    {
        CloseMenu();
    }

    public void Quit()
    {
        CloseMenu();
        if (IsSinglePlayer)
        {
            NetworkManager.StopHost();
        }
        else
        {
            if (NetworkManager.mode == Mirror.NetworkManagerMode.Host)
            {
                NetworkManager.StopHost();
            }
            else
            {
                NetworkManager.StopClient();
            }
        }
    }

    #region Singleton

    private static GameController _instance;

    public static GameController Instance { get { return _instance; } }

    #endregion
}
