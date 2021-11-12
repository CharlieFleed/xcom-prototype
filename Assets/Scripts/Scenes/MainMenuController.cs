using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] NetworkManager _networkManager;
    [SerializeField] string _multiplayerScene;

    public void SinglePlayer()
    {
        _networkManager.StartHost();
    }

    public void MultiPlayer()
    {
        Destroy(_networkManager.gameObject);
        SceneManager.LoadScene(_multiplayerScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
