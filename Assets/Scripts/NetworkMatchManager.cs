using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class NetworkMatchManager : NetworkBehaviour
{
    #region Fields

    [SerializeField] GameObject _unitPrefab;
    [SerializeField] int _NumOfUnits = 3;

    Player[] _players;

    public event Action OnNewTurn = delegate { };

    Queue<Team> _teams = new Queue<Team>();
    Character _currentCharacter;
    Queue<BattleEventGroup> _battleEventGroups = new Queue<BattleEventGroup>();

    public Character CurrentCharacter { get { return _currentCharacter; } }
    public Team ActiveTeam { get { return _teams.Peek(); } }

    public Color[] TeamColors = new Color[3] { Color.cyan, Color.red, Color.green };

    bool _matchStarted;

    #endregion

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    [ClientRpc]
    public void RpcRegisterPlayer(Player player)
    {
        Debug.Log("RpcRegisterPlayer");
        if (player == null)
            Debug.Log($"player null");
        else
            Debug.Log($"player {player}");
        Team team = new Team();
        team.Owner = player;
        team.Name = _teams.Count.ToString();
        _teams.Enqueue(team);
        //
        player.OnActionComplete += HandlePlayer_ActionComplete;
    }

    public void InstantiateUnits(Player player)
    {
        Debug.Log("InstantiateUnits");
        List<GridNode> spawnPositions = GridManager.Instance.GetSpawnPositions(_NumOfUnits);
        Team team = GetTeam(player);
        for (int c = 0; c < _NumOfUnits; c++)
        {
            GameObject unit = Instantiate(_unitPrefab, Vector3.zero, Quaternion.identity);
            unit.transform.position = spawnPositions[c].FloorPosition;
            NetworkServer.Spawn(unit, player.gameObject);
            RpcRegisterUnit(unit, player);
        }
    }

    [ClientRpc]
    void RpcRegisterUnit(GameObject unit, Player player)
    {
        Debug.Log("Registering Unit");        
        Team team = GetTeam(player);
        unit.name = team.Name + "-" + team.Characters.Count;
        if (unit.name == "0-0") unit.name = "Col. Ian McCaskill";
        team.Characters.Add(unit.GetComponent<Character>());
        unit.GetComponent<Character>().Team = team;
        // register events
        unit.GetComponent<Character>().OnActionComplete += HandleCharacter_ActionComplete;
        unit.GetComponent<Character>().OnPass += HandleCharacter_Pass;
        unit.GetComponent<Character>().OnEndTurn += HandleCharacter_EndTurn;
        // internal setup
        unit.GetComponent<GridEntity>().CurrentNode = GridManager.Instance.GetGridNodeFromWorldPosition(unit.transform.position);
    }

    [ClientRpc]
    public void RpcStartMatch()
    {
        Debug.Log("Start Match");
        Debug.Log($"_teams:{_teams.Count}");
        ActiveTeam.StartTurn();
        _matchStarted = true;
    }

    Team GetTeam(Player player)
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
        if (!_matchStarted)
            return;
        if (!AllUnitsInitialized())
            return;
        UpdateBattleEvents();
        if (_battleEventGroups.Count == 0)
        {
            if (_currentCharacter == null)
            {
                SelectNextCharacter();
                if (_currentCharacter != null)
                {
                    Debug.Log("New Turn");
                    OnNewTurn();
                    ActiveTeam.Owner.Activate();
                }
                if (_teams.Count == 0)
                {
                    Debug.Log("PANIC!PANIC!PANIC!");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }

    bool AllUnitsInitialized()
    {
        bool initialized = true;

        foreach (var team in _teams)
        {
            foreach (var character in team.Characters)
            {
                initialized &= character.Initialized;
            }
        }
        return initialized;
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

    void SelectNextCharacter()
    {
        _currentCharacter = ActiveTeam.GetFirstReadyCharacter();
        if (_currentCharacter == null)
        {
            Team team = _teams.Dequeue();
            team.EndTurn();
            _teams.Enqueue(team);
            ActiveTeam.StartTurn();
            _currentCharacter = ActiveTeam.GetFirstReadyCharacter();
            if (_currentCharacter == null)
            {
                _teams.Dequeue();
            }
        }
    }

    void HandlePlayer_ActionComplete()
    {
        _currentCharacter = null;
        ActiveTeam.Owner.Deactivate();
    }

    void HandleCharacter_ActionComplete(Character character)
    {
        if (character == _currentCharacter)
        {
            _currentCharacter = null;
            ActiveTeam.Owner.Deactivate();
        }
    }

    void HandleCharacter_Pass()
    {
        _currentCharacter = null;
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.RotateReadyCharacters();
    }

    void HandleCharacter_EndTurn()
    {
        _currentCharacter = null;
        ActiveTeam.Owner.Deactivate();
        ActiveTeam.EndTurn();
    }

    public List<T> GetEnemiesAs<T>(Character character)
    {
        List<T> enemies = new List<T>();
        foreach (Team team in _teams)
        {
            if (team != character.Team)
            {
                foreach (Character enemy in team.Characters)
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

    public List<GridEntity> GetGridEntities()
    {
        List<GridEntity> gridEntities = new List<GridEntity>();
        gridEntities.AddRange(FindObjectsOfType<GridEntity>());
        return gridEntities;
    }

    private void OnDrawGizmos()
    {
        if (_currentCharacter != null)
        {
            foreach (Team team in _teams)
            {
                if (!team.Characters.Contains(_currentCharacter))
                {
                    foreach (Character character in team.Characters)
                    {
                        Ray ray;
                        float rayLength;
                        if (GridCoverManager.Instance.LineOfSight(_currentCharacter.GetComponent<GridEntity>(), character.GetComponent<GridEntity>(), out ray, out rayLength, new List<GridNode[]>()))
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
