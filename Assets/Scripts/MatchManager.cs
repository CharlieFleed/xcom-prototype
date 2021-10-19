using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    #region Fields

    [SerializeField] GameObject _unitPrefab;
    [SerializeField] int _NumOfUnits = 3;
    [SerializeField] Player[] _players;

    public event Action OnNewTurn = delegate { };

    Queue<Team> _teams = new Queue<Team>();
    Character _currentCharacter;
    Queue<BattleEventGroup> _battleEventGroups = new Queue<BattleEventGroup>();

    public Character CurrentCharacter { get { return _currentCharacter; } }
    public Team ActiveTeam { get { return _teams.Peek(); } }

    #endregion

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateTeams();
        GridManager.Instance.PositionPlayers(_teams);
        ActiveTeam.StartTurn();
    }

    private void Update()
    {
        UpdateBattleEvents();
        if (_battleEventGroups.Count == 0)
        {
            if (_currentCharacter == null)
            {
                SelectNextCharacter();
                if (_currentCharacter != null)
                {
                    OnNewTurn();
                    ActiveTeam.Owner.Activate();
                }
                if (_teams.Count == 0)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
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

    void GenerateTeams()
    {
        _teams.Clear();
        Color[] teamColors = new Color[3] { Color.cyan, Color.red, Color.green };
        for (int t = 0; t < _players.Length; t++)
        {
            Team team = new Team();
            team.Owner = _players[t];
            _players[t].OnActionComplete += HandlePlayer_ActionComplete;
            for (int c = 0; c < _NumOfUnits; c++)
            {
                GameObject unit = Instantiate(_unitPrefab, Vector3.zero, Quaternion.identity);
                unit.name = t + "-" + c;
                if (t == 0 && c == 0) unit.name = "Col. Ian McCaskill";
                team.Characters.Add(unit.GetComponent<Character>());
                unit.GetComponent<Character>().Team = team;
                unit.GetComponentInChildren<MeshRenderer>().material.color = teamColors[t];
                // register events
                unit.GetComponent<Character>().OnActionComplete += HandleCharacter_ActionComplete;
                unit.GetComponent<Character>().OnPass += HandleCharacter_Pass;
                unit.GetComponent<Character>().OnEndTurn += HandleCharacter_EndTurn;
            }
            _teams.Enqueue(team);
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

    void HandleCharacter_ActionComplete()
    {
        _currentCharacter = null;
        ActiveTeam.Owner.Deactivate();
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

    private static MatchManager _instance;

    public static MatchManager Instance { get { return _instance; } }

    #endregion
}
