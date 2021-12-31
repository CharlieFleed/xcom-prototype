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
        _pauseMenu.GetComponent<Image>().CrossFadeAlpha(0, 0, true);
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
        OpenMenu();
        Pause();
    }

    public void HandlePauseClick()
    {
        OpenMenu();
        Pause();
    }

    public void HandleResumeClick()
    {
        StartCoroutine(CloseMenuAndResume());
    }

    private void Update()
    {
        if (GamePaused && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(CloseMenuAndResume());
        }
    }

    void OpenMenu()
    {
        _pauseMenu.SetActive(true);
        _pauseMenu.GetComponent<Image>().CrossFadeAlpha(.5f, .25f, true);
    }

    private static void Pause()
    {
        Time.timeScale = 0;
        GamePaused = true;
    }

    void CloseMenu()
    {
        _pauseMenu.SetActive(false);
        _pauseMenu.GetComponent<Image>().CrossFadeAlpha(0, 0.02f, true);
    }

    private static void Resume()
    {
        Time.timeScale = 1;
        GamePaused = false;
    }

    public IEnumerator CloseMenuAndResume()
    {
        yield return null;
        CloseMenu();
        Resume();
    }

    public void Quit()
    {
        Debug.Log("Quit");
        CloseMenu();
        Resume();
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
