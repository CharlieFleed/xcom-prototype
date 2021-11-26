using UnityEngine;
using System.Collections;
using System;

public class InputCache
{
    bool[] _keysDown = new bool[Enum.GetNames(typeof(KeyCode)).Length];
    bool[] _mouseButtonsDown = new bool[2];

    public void Update()
    {
        for (int i = 0; i < _keysDown.Length; i++)
        {
            _keysDown[i] = Input.GetKeyDown((KeyCode)i);
        }
        for (int i = 0; i < _mouseButtonsDown.Length; i++)
        {
            _mouseButtonsDown[i] = Input.GetMouseButtonDown(i);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _keysDown.Length; i++)
        {
            _keysDown[i] = false;
        }
        for (int i = 0; i < _mouseButtonsDown.Length; i++)
        {
            _mouseButtonsDown[i] = false;
        }
    }

    public bool GetKeyDown(KeyCode keyCode)
    {
        return _keysDown[(int)keyCode];
    }

    public bool GetMouseButtonDown(int button)
    {
        return _mouseButtonsDown[button];
    }
}
