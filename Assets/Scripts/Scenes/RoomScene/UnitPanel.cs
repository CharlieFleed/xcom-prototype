using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour
{
    public UnitSelector UnitSelector;
    [SerializeField] Text _text;
    public GameObject Buttons;

    public void Start()
    {
        _text.text = UnitSelector.Name();
    }

    public void Left()
    {
        UnitSelector.Left();
        _text.text = UnitSelector.Name();
    }
    public void Right()
    {
        UnitSelector.Right();
        _text.text = UnitSelector.Name();
    }

    public void UpdateName()
    {
        _text.text = UnitSelector.Name();
    }
}
