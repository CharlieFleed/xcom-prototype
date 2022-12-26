using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    [SerializeField] GameObject[] _units;

    public MyNetworkRoomPlayer Player;
    public int UnitPosition;

    int _unitClass = 0;

    private void Start()
    {
        for (int i = 0; i < _units.Length; i++)
        {
            _units[i].SetActive(i == _unitClass);
        }
    }

    public void Left()
    {
        _unitClass = (_unitClass + _units.Length - 1) % _units.Length;
        UpdateUnit();
    }

    public void Right()
    {
        _unitClass = (_unitClass + 1) % _units.Length;
        UpdateUnit();
    }

    void UpdateUnit()
    {
        for (int i = 0; i < _units.Length; i++)
        {
            _units[i].SetActive(i == _unitClass);
        }
        Player.SetUnitType(UnitPosition, _unitClass);
    }

    public string Name()
    {
        return _units[_unitClass].name;
    }

    internal void SetUnitClass(int unitClass)
    {
        _unitClass = unitClass;
        for (int i = 0; i < _units.Length; i++)
        {
            _units[i].SetActive(i == _unitClass);
        }
    }
}
