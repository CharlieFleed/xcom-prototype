using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField] GameObject _unitSelectorPrefab;
    [SerializeField] GameObject _unitPanelsRowPrefab;

    [SerializeField] GameObject _panel;

    Dictionary<MyNetworkRoomPlayer, GameObject> _unitPanelsRows = new Dictionary<MyNetworkRoomPlayer, GameObject>();

    private void Awake()
    {
        //Debug.Log("RoomController Awake");
        _instance = this;
    }

    public void AddPlayerGUI(MyNetworkRoomPlayer player)
    {
        //Debug.Log("AddPlayerGUI");
        if (_unitPanelsRows.ContainsKey(player))
        {
            //Debug.Log("player GUI exists");
            return;
        }
        GameObject unitPanelsRow = Instantiate(_unitPanelsRowPrefab, _panel.transform);
        unitPanelsRow.GetComponent<RoomPlayerUI>().SetPlayer(player);
        for (int i = 0; i < 4; i++)
        {
            GameObject unitSelector = Instantiate(_unitSelectorPrefab);
            unitSelector.transform.position = new Vector3(-3 + i * 3, .5f - _unitPanelsRows.Count * 3.5f, 0);
            unitSelector.GetComponent<UnitSelector>().UnitPosition = i;
            unitSelector.GetComponent<UnitSelector>().Player = player;
            unitSelector.GetComponent<UnitSelector>().SetUnitClass(player.MatchSettings.unitClasses[i]);
            unitPanelsRow.GetComponent<RoomPlayerUI>().UnitPanels[i].GetComponent<UnitPanel>().UnitSelector = unitSelector.GetComponent<UnitSelector>();
            unitPanelsRow.GetComponent<RoomPlayerUI>().UnitPanels[i].GetComponent<UnitPanel>().UpdateName();
        }
        _unitPanelsRows.Add(player, unitPanelsRow);
        player.OnUnitClassChange += Player_OnUnitClassChange;
    }

    private void Player_OnUnitClassChange(MyNetworkRoomPlayer player, int unitPosition, int unitClass)
    {
        if (_unitPanelsRows.ContainsKey(player))
        {
            UnitPanel[] unitPanels = _unitPanelsRows[player].GetComponentsInChildren<UnitPanel>();
            unitPanels[unitPosition].UnitSelector.SetUnitClass(unitClass);
            unitPanels[unitPosition].UpdateName();
        }
    }

    public void RemovePlayerGUI(MyNetworkRoomPlayer player)
    {
        if (_unitPanelsRows.ContainsKey(player))
        {
            GameObject unitPanelRow = _unitPanelsRows[player];
            _unitPanelsRows.Remove(player);
            player.OnUnitClassChange -= Player_OnUnitClassChange;
            UnitPanel[] unitPanels = unitPanelRow.GetComponentsInChildren<UnitPanel>();
            for (int i = 0; i < unitPanels.Length; i++)
            {
                Destroy(unitPanels[i].UnitSelector.gameObject);
                Destroy(unitPanels[i]);
            }
            Destroy(unitPanelRow);
        }
    }

    private void OnDestroy()
    {
        foreach (var player in _unitPanelsRows.Keys)
        {
            player.OnUnitClassChange -= Player_OnUnitClassChange;
        }
        _unitPanelsRows.Clear();
    }

    #region Singleton

    private static RoomController _instance;

    public static RoomController Instance { get { return _instance; } }

    #endregion

}
