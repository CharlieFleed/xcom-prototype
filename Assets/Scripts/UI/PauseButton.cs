using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    [SerializeField] GameController _gameController;

    public event Action OnClick = delegate { };

    private void OnEnable()
    {
        OnClick += _gameController.HandlePauseClick;
    }

    private void OnDisable()
    {
        OnClick -= _gameController.HandlePauseClick;
    }

    public void Click()
    {
        OnClick();
    }
}
