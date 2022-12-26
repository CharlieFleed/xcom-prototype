using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBuilder : NetworkBehaviour
{
    [SerializeField] int _NumOfUnits = 3;
    [SerializeField] GameObject[] _unitPrefabs;
    [SerializeField] NetworkMatchManager _networkMatchManager;

    List<Team> _teams = new List<Team>();

    private void Awake()
    {
        _instance = this;
    }

    [ClientRpc]
    public void RpcRegisterPlayer(MyGamePlayer player)
    {
        Debug.Log("RpcRegisterPlayer");
        RegisterPlayer(player);
        if (isServer)
        {
            InstantiateUnits(player);
        }
    }

    public void RegisterPlayer(MyGamePlayer player)
    {
        Debug.Log("RegisterPlayer");
        Team team = new Team();
        team.Owner = player;
        team.Id = _teams.Count;
        team.Name = _teams.Count.ToString();
        _teams.Add(team);
        _networkMatchManager.AddTeam(team);
    }

    public void SinglePlayerMatchSetup(MyGamePlayer player)
    {
        int[][] unitClasses = new int[2][];
        unitClasses[0] = new int[] { 1, 2, 0, 3, 4, 5 };
        unitClasses[1] = new int[] { 0, 2, 3, 4, 2, 5 };
        for (int i = 0; i < 2; i++)
        {
            RegisterPlayer(player);
            Team team = _teams.ToArray()[i];
            team.IsAI = (i == 1);
            Squad squad = new Squad();
            List<GridNode> spawnPositions = GridManager.Instance.GetSpawnPositions(team.Id, _NumOfUnits);
            for (int c = 0; c < _NumOfUnits; c++)
            {
                GameObject unitObj = Instantiate(_unitPrefabs[unitClasses[i][c]], Vector3.zero, Quaternion.identity);
                unitObj.name = team.Name + "-" + team.Members.Count;
                unitObj.transform.position = spawnPositions[c].FloorPosition;
                NetworkServer.Spawn(unitObj, player.gameObject);
                // Registering Unit
                team.Members.Add(unitObj.GetComponent<Unit>());
                unitObj.GetComponent<Unit>().Team = team;
                squad.Units.Add(unitObj.GetComponent<SquadUnit>());
                unitObj.GetComponent<SquadUnit>().Squad = squad;
                _networkMatchManager.RegisterUnit(unitObj);
                // AI
                if (team.IsAI)
                {
                    unitObj.GetComponent<UnitStateMachine>().enabled = true;
                }
            }
        }
    }

    public void InstantiateUnits(MyGamePlayer player)
    {
        Debug.Log("InstantiateUnits");
        Team team = GetTeam(player);
        //Debug.Log($"Team {team.Id}");
        List<GridNode> spawnPositions = GridManager.Instance.GetSpawnPositions(team.Id, _NumOfUnits);
        for (int c = 0; c < _NumOfUnits; c++)
        {
            GameObject unit = Instantiate(_unitPrefabs[player.MatchSettings.unitClasses[c]], Vector3.zero, Quaternion.identity);
            unit.transform.position = spawnPositions[c].FloorPosition;
            NetworkServer.Spawn(unit, player.gameObject);
            RpcRegisterUnit(unit, player);
        }
    }

    [ClientRpc]
    void RpcRegisterUnit(GameObject unit, MyGamePlayer player)
    {
        //Debug.Log("Registering Unit");        
        Team team = GetTeam(player);
        unit.name = team.Name + "-" + team.Members.Count;
        team.Members.Add(unit.GetComponent<Unit>());
        unit.GetComponent<Unit>().Team = team;
        _networkMatchManager.RegisterUnit(unit);
    }

    Team GetTeam(MyGamePlayer player)
    {
        Team team = null;
        foreach (var t in _teams)
        {
            if (t.Owner == player)
            {
                team = t;
            }
        }
        return team;
    }

    #region Singleton

    private static MatchBuilder _instance;

    public static MatchBuilder Instance { get { return _instance; } }

    #endregion
}
