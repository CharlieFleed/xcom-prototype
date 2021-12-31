using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum MatchState {Setup, Started, End};
public enum TurnState { Idle, Action, ActionComplete, End};

public class NetworkMatchManager : NetworkBehaviour
{
    #region Fields

    [SerializeField] GameObject _unitPrefab;
    [SerializeField] int _NumOfUnits = 3;
    [SerializeField] GameObject[] _unitPrefabs;

    MyGamePlayer[] _players;

    public event Action OnBeforeTurnBegin = delegate { };
    public event Action OnTurnBegin = delegate { };
    public event Action OnActionComplete = delegate { };
    public event Action OnPause = delegate { };

    List<Team> _teams = new List<Team>();
    Unit _currentUnit;
    List<BattleEventGroup> _battleEventGroups = new List<BattleEventGroup>();

    public Unit CurrentUnit { get { return _currentUnit; } }

    Team ActiveTeam { get { return _teams[0]; } }

    public Color[] TeamColors = new Color[3] { Color.cyan, Color.red, Color.green };

    MatchState _state = MatchState.Setup;
    public MatchState State { get { return _state; } }

    TurnState _turnState = TurnState.Idle;

    Team _winner = null;
    public Team Winner { get { return _winner; } }

    bool _armorsSet;

    InputCache _input = new InputCache();

    #endregion

    private void Awake()
    {
        //Debug.Log("NetworkMatchManager Awake");
        _instance = this;
    }

    public void SinglePlayerMatchSetup(MyGamePlayer player)
    {
        int[][] unitClasses = new int[2][];
        unitClasses[0] = new int[] { 0, 0, 0, 0 };
        unitClasses[1] = new int[] { 0, 0, 0, 0 };
        for (int i = 0; i < 2; i++)
        {
            RegisterPlayer(player);
            Team team = _teams.ToArray()[i];
            team.IsAI = (i == 1);
            Squad squad = new Squad();
            List<GridNode> spawnPositions = GridManager.Instance.GetSpawnPositions(team.Id, _NumOfUnits);
            for (int c = 0; c < _NumOfUnits; c++)
            {
                GameObject unitObj = Instantiate(_unitPrefabs[player.MatchSettings.unitClasses[unitClasses[i][c]]], Vector3.zero, Quaternion.identity);
                unitObj.name = team.Name + "-" + team.Members.Count;
                unitObj.transform.position = spawnPositions[c].FloorPosition;
                NetworkServer.Spawn(unitObj, player.gameObject);
                // Registering Unit
                team.Members.Add(unitObj.GetComponent<Unit>());
                unitObj.GetComponent<Unit>().Team = team;
                squad.Units.Add(unitObj.GetComponent<SquadUnit>());
                unitObj.GetComponent<SquadUnit>().Squad = squad;
                // register events
                unitObj.GetComponent<ActionsController>().OnActionComplete += HandleActionsController_ActionComplete;
                unitObj.GetComponent<UnitLocalController>().OnPass += HandleUnitLocalController_Pass;
                unitObj.GetComponent<UnitLocalController>().OnEndTurn += HandleUnitLocalController_EndTurn;
                unitObj.GetComponent<UnitLocalController>().OnPause += HandleUnitLocalController_Pause;
                // internal setup
                unitObj.GetComponent<GridEntity>().CurrentNode = GridManager.Instance.GetGridNodeFromWorldPosition(unitObj.transform.position);
                // AI
                if (team.IsAI)
                {
                    unitObj.GetComponent<UnitStateMachine>().enabled = true;
                }
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
        // register events
        unit.GetComponent<ActionsController>().OnActionComplete += HandleActionsController_ActionComplete;
        unit.GetComponent<UnitLocalController>().OnPass += HandleUnitLocalController_Pass;
        unit.GetComponent<UnitLocalController>().OnEndTurn += HandleUnitLocalController_EndTurn;
        unit.GetComponent<UnitLocalController>().OnPause += HandleUnitLocalController_Pause;
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
        _input.Update();
    }

    private void LateUpdate()
    {
        if (_state == MatchState.Setup || _state == MatchState.End)
            return;
        if (!AllUnitsStarted())
            return;
        if (!_armorsSet)
        {
            SetRandomArmors();
            return;
        }
        UpdateTurn();
        if (_currentUnit == null)
        {
            if (_input.GetKeyDown(KeyCode.Escape))
            {
                OnPause();
            }
        }
        _input.Clear();
    }

    void UpdateTurn()
    {
        switch (_turnState)
        {
            case TurnState.Idle:
                SelectNextUnit();
                OnBeforeTurnBegin();
                OnTurnBegin();
                ActiveTeam.Owner.Activate();
                BattleEventUnitAction battleEventEndTurn = new BattleEventUnitAction(this);
                AddBattleEvent(battleEventEndTurn, true, 1);
                _turnState = TurnState.Action;
                break;
            case TurnState.Action:
                // transition to ActionComplete triggered by handlers
                UpdateBattleEvents();
                break;
            case TurnState.ActionComplete:
                UpdateBattleEvents();
                if (_battleEventGroups.Count == 0)
                {
                    _turnState = TurnState.End;
                }
                break;
            case TurnState.End:
                UpdateBattleEvents();
                if (_battleEventGroups.Count == 0)
                {
                    RemoveDefeatedTeams();
                    CheckEndBattle();
                    _turnState = TurnState.Idle;
                }
                break;
        }
    }

    bool AllUnitsStarted()
    {
        bool started = true;

        foreach (var team in _teams)
        {
            foreach (var unit in team.Members)
            {
                started &= unit.Started;
            }
        }
        return started;
    }

    void SetRandomArmors()
    {
        if (NetworkRandomGenerator.Instance.Ready())
        {
            foreach (var team in _teams)
            {
                foreach (var unit in team.Members)
                {
                    unit.GetComponent<Armor>().SetValue(NetworkRandomGenerator.Instance.RandomRange(1, 4));
                }
            }
            _armorsSet = true;
        }
    }

    void ActionComplete()
    {
        _turnState = TurnState.ActionComplete;
        OnActionComplete();
    }

    bool CheckEndBattle()
    {
        if (_teams.Count == 1)
        {
            _winner = _teams[0];
            _state = MatchState.End;
            return true;
        }
        if (_teams.Count == 0)
        {
            _state = MatchState.End;
            return true;
        }
        return false;
    }   

    void UpdateBattleEvents()
    {
        //Debug.Log($"BattleEventGroups: {_battleEventGroups.Count}.");
        //for (int i = 0; i < _battleEventGroups.Count; i++)
        //{
        //    Debug.Log($"{i} {_battleEventGroups[i].ToString()}.");
        //}
        if (_battleEventGroups.Count > 0)
        {
            _battleEventGroups[0].Run();
            if (_battleEventGroups[0].IsComplete())
            {
                //Debug.Log("RemoveAt(0).");
                _battleEventGroups.RemoveAt(0);
            }
        }
    }

    public void AddBattleEvent(BattleEvent battleEvent, bool newGroup, int priority)
    {
        //Debug.Log($"Adding {battleEvent.GetType().ToString()} with priority {priority}.");
        if (!newGroup)
        {
            // find an existing group at the same priority
            for (int i = 0; i < _battleEventGroups.Count; i++)
            {
                if (_battleEventGroups[i].Priority == priority)
                {
                    _battleEventGroups[i].AddBattleEvent(battleEvent);
                    //Debug.Log("added to existing group");
                    return;
                }
            }
        }
        // if we got here we need to create a new group
        BattleEventGroup battleEventGroup = new BattleEventGroup(priority);
        int index = 0;
        for (; index < _battleEventGroups.Count; index++)
        {
            if (priority >= _battleEventGroups[index].Priority)
            {
                break;
            }
        }
        _battleEventGroups.Insert(index, battleEventGroup);
        battleEventGroup.AddBattleEvent(battleEvent);
    }

    void SelectNextUnit()
    {
        _currentUnit = ActiveTeam.GetFirstReadyMember();
        if (_currentUnit == null)
        {
            ActiveTeam.EndTurn();
            RotateTeams();
            ActiveTeam.StartTurn();
            _currentUnit = ActiveTeam.GetFirstReadyMember();
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

    #region Handlers

    void HandleActionsController_ActionComplete(Unit unit)
    {
        if (unit == _currentUnit)
        {
            ActionComplete();
            ActiveTeam.Owner.Deactivate();
        }
    }

    void HandleUnitLocalController_Pass()
    {
        ActionComplete();
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.RotateReadyMembers();
    }

    void HandleUnitLocalController_EndTurn()
    {
        ActionComplete();
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.EndTurn();
    }

    void HandleUnitLocalController_Pause()
    {
        OnPause();
    }

    #endregion

    public List<T> GetEnemiesAs<T>(Unit teamMember)
    {
        List<T> enemies = new List<T>();
        foreach (Team team in _teams)
        {
            if (team != teamMember.Team)
            {
                foreach (Unit enemy in team.Members)
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
                foreach (Unit friend in team.Members)
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
                if (!team.Members.Contains(_currentUnit))
                {
                    foreach (Unit unit in team.Members)
                    {
                        List<GridNode[]> losPoints = new List<GridNode[]>();
                        if (GridCoverManager.Instance.LineOfSight(_currentUnit.GetComponent<GridEntity>(), unit.GetComponent<GridEntity>(), out Ray ray, out float rayLength, losPoints))
                        {
                            Gizmos.color = Color.green;
                            Ray ray2 = new Ray(losPoints[0][0].FloorPosition, losPoints[0][1].FloorPosition - losPoints[0][0].FloorPosition);
                            float rayLength2 = (losPoints[0][1].FloorPosition - losPoints[0][0].FloorPosition).magnitude;
                            Gizmos.DrawRay(ray2.origin, ray2.direction * rayLength2);
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
