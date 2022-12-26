using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum MatchState {Setup, Started, End};
public enum TurnState { Idle, UnitAction, UnitActionComplete, End};

public class NetworkMatchManager : NetworkBehaviour
{
    #region Fields

    public event Action OnBeforeTurnBegin = delegate { };
    public event Action OnTurnBegin = delegate { };
    public event Action OnCompleteUnitAction = delegate { };
    public event Action OnPause = delegate { };

    List<Team> _teams = new List<Team>();
    List<BattleEventGroup> _battleEventGroups = new List<BattleEventGroup>();

    Unit _currentUnit;
    public Unit CurrentUnit { get { return _currentUnit; } }

    Team ActiveTeam { get { return _teams[0]; } }

    MatchState _state = MatchState.Setup;
    public MatchState State { get { return _state; } }

    TurnState _turnState = TurnState.Idle;

    Team _winner = null;
    public Team Winner { get { return _winner; } }

    bool _armorsSet;

    InputCache _inputCache = new InputCache();

    #endregion

    private void Awake()
    {
        //Debug.Log("NetworkMatchManager Awake");
        _instance = this;
    }    

    public void RegisterUnit(GameObject unit)
    {
        // register events
        unit.GetComponent<ActionsController>().OnActionComplete += HandleActionsController_ActionComplete; //TODO: unregister
        unit.GetComponent<UnitLocalController>().OnPass += HandleUnitLocalController_Pass; //TODO: unregister
        unit.GetComponent<UnitLocalController>().OnEndTurn += HandleUnitLocalController_EndTurn; //TODO: unregister
        unit.GetComponent<UnitLocalController>().OnPause += HandleUnitLocalController_Pause; //TODO: unregister
        // internal setup
        unit.GetComponent<GridEntity>().CurrentNode = GridManager.Instance.GetGridNodeFromWorldPosition(unit.transform.position);
    }

    public void AddTeam(Team team)
    {
        _teams.Add(team);
    }

    [ClientRpc]
    public void RpcStartMatch()
    {
        //Debug.Log("Start Match");
        //Debug.Log($"_teams:{_teams.Count}");
        _state = MatchState.Started;
    }    

    private void Update()
    {
        _inputCache.Update();
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
            if (_inputCache.GetKeyDown(KeyCode.Escape))
            {
                OnPause();
            }
        }
        _inputCache.Clear();
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

    void UpdateTurn()
    {
        switch (_turnState)
        {
            case TurnState.Idle:
                SelectNextUnit();
                OnBeforeTurnBegin();
                OnTurnBegin();
                ActiveTeam.Owner.Activate();
                BattleEventUnitAction battleEventUnitAction = new BattleEventUnitAction(this);
                AddBattleEvent(battleEventUnitAction, 1, BattleEvent.CreateNewGroup);
                _turnState = TurnState.UnitAction;
                break;
            case TurnState.UnitAction:
                // transition to ActionComplete triggered by handlers
                UpdateBattleEvents();
                break;
            case TurnState.UnitActionComplete:
                UpdateBattleEvents();
                // transition to End when all battle events are completed
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

    void CompleteUnitAction()
    {
        _turnState = TurnState.UnitActionComplete;
        OnCompleteUnitAction();
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
        //string log = "";
        //log += $"BattleEventGroups: {_battleEventGroups.Count}\n";
        //for (int i = 0; i < _battleEventGroups.Count; i++)
        //{
        //    log += $"Position: {i} {_battleEventGroups[i].ToString()}\n";
        //}
        //Debug.Log(log);
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

    public void AddBattleEvent(BattleEvent battleEvent, int priority = -1, bool createNewGroup = false)
    {
        //Debug.Log($"Adding {battleEvent.GetType().ToString()} with priority {priority}.");
        if (!createNewGroup)
        {
            // find an existing group at the same priority
            for (int i = 0; i < _battleEventGroups.Count; i++)
            {
                if (_battleEventGroups[i].Priority == priority || priority == -1)
                {
                    _battleEventGroups[i].AddBattleEvent(battleEvent);
                    //Debug.Log("added to existing group");
                    return;
                }
            }
        }
        // if we got here we need to create a new group
        BattleEventGroup newBattleEventGroup = new BattleEventGroup(priority);
        //Debug.Log($"Created new group with priority {priority}.");
        int index = 0;
        for (; index < _battleEventGroups.Count; index++)
        {
            if (priority > _battleEventGroups[index].Priority)
            {
                break;
            }
        }
        _battleEventGroups.Insert(index, newBattleEventGroup);
        newBattleEventGroup.AddBattleEvent(battleEvent);
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
            CompleteUnitAction();
            ActiveTeam.Owner.Deactivate();
        }
    }

    void HandleUnitLocalController_Pass()
    {
        CompleteUnitAction();
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.RotateReadyMembers();
    }

    void HandleUnitLocalController_EndTurn()
    {
        CompleteUnitAction();
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
