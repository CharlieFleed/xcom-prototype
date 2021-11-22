using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum MatchState {Setup, Started, End};

public class NetworkMatchManager : NetworkBehaviour
{
    #region Fields

    [SerializeField] GameObject _unitPrefab;
    [SerializeField] int _NumOfUnits = 3;
    [SerializeField] GameObject[] _unitPrefabs;

    MyGamePlayer[] _players;

    public event Action OnBeforeTurnBegin = delegate { };
    public event Action OnTurnBegin = delegate { };
    public event Action OnTurnEnd = delegate { };
    public event Action OnPause = delegate { };

    List<Team> _teams = new List<Team>();
    Unit _currentUnit;
    Queue<BattleEventGroup> _battleEventGroups = new Queue<BattleEventGroup>();

    public Unit CurrentUnit { get { return _currentUnit; } }
    public Team ActiveTeam { get { return _teams[0]; } }

    public Color[] TeamColors = new Color[3] { Color.cyan, Color.red, Color.green };

    MatchState _state = MatchState.Setup;
    public MatchState State { get { return _state; } }
    Team _winner = null;
    public Team Winner { get { return _winner; } }

    bool _armorsSet;

    #endregion

    private void Awake()
    {
        //Debug.Log("NetworkMatchManager Awake");
        _instance = this;
    }

    public void SinglePlayerMatchSetup(MyGamePlayer player)
    {
        for (int i = 0; i < 2; i++)
        {
            RegisterPlayer(player);
            Team team = _teams.ToArray()[i];
            List<GridNode> spawnPositions = GridManager.Instance.GetSpawnPositions(team.Id, _NumOfUnits);
            for (int c = 0; c < _NumOfUnits; c++)
            {
                GameObject unit = Instantiate(_unitPrefabs[player.MatchSettings.unitClasses[c]], Vector3.zero, Quaternion.identity);
                unit.name = team.Name + "-" + team.Units.Count;
                unit.transform.position = spawnPositions[c].FloorPosition;
                NetworkServer.Spawn(unit, player.gameObject);
                // Registering Unit
                team.Units.Add(unit.GetComponent<Unit>());
                unit.GetComponent<Unit>().Team = team;
                // register events
                unit.GetComponent<Unit>().OnActionComplete += HandleUnit_ActionComplete;
                unit.GetComponent<Unit>().OnPass += HandleUnit_Pass;
                unit.GetComponent<Unit>().OnEndTurn += HandleUnit_EndTurn;
                unit.GetComponent<Unit>().OnPause += HandleUnit_OnPause;
                // internal setup
                unit.GetComponent<GridEntity>().CurrentNode = GridManager.Instance.GetGridNodeFromWorldPosition(unit.transform.position);
            }
        }
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
        //
        player.OnActionComplete += HandlePlayer_ActionComplete;
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
        unit.name = team.Name + "-" + team.Units.Count;
        team.Units.Add(unit.GetComponent<Unit>());
        unit.GetComponent<Unit>().Team = team;
        // register events
        unit.GetComponent<Unit>().OnActionComplete += HandleUnit_ActionComplete;
        unit.GetComponent<Unit>().OnPass += HandleUnit_Pass;
        unit.GetComponent<Unit>().OnEndTurn += HandleUnit_EndTurn;
        unit.GetComponent<Unit>().OnPause += HandleUnit_OnPause;
        // internal setup
        unit.GetComponent<GridEntity>().CurrentNode = GridManager.Instance.GetGridNodeFromWorldPosition(unit.transform.position);
    }

    [ClientRpc]
    public void RpcStartMatch()
    {
        //Debug.Log("Start Match");
        //Debug.Log($"_teams:{_teams.Count}");
        _state = MatchState.Started;
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

    private void Update()
    {
        if (_state == MatchState.Setup || _state == MatchState.End)
            return;
        if (!AllUnitsInitialized())
            return;
        if (!_armorsSet)
        {
            SetRandomArmors();
            return;
        }
        UpdateBattleEvents();
        if (_battleEventGroups.Count == 0)
        {
            if (_currentUnit == null)
            {
                RemoveDefeatedTeams();
                if (_teams.Count == 1)
                {
                    _winner = _teams[0];
                    _state = MatchState.End;
                    return;
                }
                if (_teams.Count == 0)
                {
                    _state = MatchState.End;
                    return;
                }
                SelectNextUnit();
                if (_currentUnit != null)
                {
                    //Debug.Log("New Turn");
                    OnBeforeTurnBegin();
                    OnTurnBegin();
                    ActiveTeam.Owner.Activate();
                }
            }
        }
        if (_currentUnit == null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnPause();
            }
        }
    }

    bool AllUnitsInitialized()
    {
        bool initialized = true;

        foreach (var team in _teams)
        {
            foreach (var unit in team.Units)
            {
                initialized &= unit.Initialized;
            }
        }
        return initialized;
    }

    void SetRandomArmors()
    {
        if (NetworkRandomGenerator.Instance.Ready())
        {
            foreach (var team in _teams)
            {
                foreach (var unit in team.Units)
                {
                    unit.GetComponent<Armor>().SetValue(NetworkRandomGenerator.Instance.RandomRange(1, 4));
                }
            }
            _armorsSet = true;
        }
    }

    void UpdateBattleEvents()
    {
        if (_battleEventGroups.Count > 0)
        {
            _battleEventGroups.Peek().Run();
            if (_battleEventGroups.Peek().IsComplete())
            {
                _battleEventGroups.Dequeue();
            }
        }
    }

    public void AddBattleEvent(BattleEvent battleEvent, bool newPhase)
    {
        if (newPhase)
        {
            BattleEventGroup battlePhase = new BattleEventGroup();
            _battleEventGroups.Enqueue(battlePhase);
            battlePhase.AddBattleEvent(battleEvent);
        }
        else
        {
            _battleEventGroups.Peek().AddBattleEvent(battleEvent);
        }
    }

    void SelectNextUnit()
    {
        _currentUnit = ActiveTeam.GetFirstReadyUnit();
        if (_currentUnit == null)
        {
            ActiveTeam.EndTurn();
            RotateTeams();
            ActiveTeam.StartTurn();
            _currentUnit = ActiveTeam.GetFirstReadyUnit();
        }
    }

    public void RotateTeams()
    {
        Team team = _teams[0];
        _teams.RemoveAt(0);
        _teams.Add(team);
    }

    void RemoveDefeatedTeams()
    {
        for (int i = _teams.Count - 1; i >= 0; i--)
        {
            if (_teams[i].IsDefeated())
            {
                _teams.RemoveAt(i);
            }
        }
    }

    void HandlePlayer_ActionComplete()
    {
        _currentUnit = null;
        ActiveTeam.Owner.Deactivate();
    }

    void HandleUnit_ActionComplete(Unit unit)
    {
        if (unit == _currentUnit)
        {
            _currentUnit = null;
            ActiveTeam.Owner.Deactivate();
        }
    }

    void HandleUnit_Pass()
    {
        _currentUnit = null;
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.RotateReadyUnits();
    }

    void HandleUnit_EndTurn()
    {
        _currentUnit = null;
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.EndTurn();
    }

    void HandleUnit_OnPause()
    {
        OnPause();
    }

    public List<T> GetEnemiesAs<T>(Unit unit)
    {
        List<T> enemies = new List<T>();
        foreach (Team team in _teams)
        {
            if (team != unit.Team)
            {
                foreach (Unit enemy in team.Units)
                {
                    if (!enemy.GetComponent<Health>().IsDead)
                    {
                        enemies.Add(enemy.GetComponent<T>());
                    }
                }
            }
        }
        return enemies;
    }

    public List<T> GetFriendsAs<T>(Unit unit)
    {
        List<T> friends = new List<T>();
        foreach (Team team in _teams)
        {
            if (team == unit.Team)
            {
                foreach (Unit friend in team.Units)
                {
                    if (!friend.GetComponent<Health>().IsDead)
                    {
                        friends.Add(friend.GetComponent<T>());
                    }
                }
            }
        }
        return friends;
    }

    public List<GridEntity> GetGridEntities()
    {
        List<GridEntity> gridEntities = new List<GridEntity>();
        gridEntities.AddRange(FindObjectsOfType<GridEntity>());
        return gridEntities;
    }

    private void OnDrawGizmos()
    {
        if (_currentUnit != null)
        {
            foreach (Team team in _teams)
            {
                if (!team.Units.Contains(_currentUnit))
                {
                    foreach (Unit unit in team.Units)
                    {
                        Ray ray;
                        float rayLength;
                        if (GridCoverManager.Instance.LineOfSight(_currentUnit.GetComponent<GridEntity>(), unit.GetComponent<GridEntity>(), out ray, out rayLength, new List<GridNode[]>()))
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawRay(ray.origin, ray.direction * rayLength);
                        }
                        else
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawRay(ray.origin, ray.direction * rayLength);
                        }
                    }
                }
            }
        }
    }

    #region Singleton

    private static NetworkMatchManager _instance;

    public static NetworkMatchManager Instance { get { return _instance; } }

    #endregion
}
