using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MatchManager : MonoBehaviour
{
    #region Fields

    [SerializeField] GameObject _playerPrefab;
    [SerializeField] int _NumOfTeams = 3;
    [SerializeField] int _NumOfPlayers = 3;

    public event Action<Character> OnCharacterActivated = delegate { };

    Queue<Team> _teams = new Queue<Team>();
    Character _activeCharacter;
    Queue<BattleEventGroup> _battleEventGroups = new Queue<BattleEventGroup>();

    public Character ActiveCharacter { get { return _activeCharacter; } }

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
        StartTeamTurn();
    }

    private void Update()
    {
        UpdateBattlePhases();
        if (_battleEventGroups.Count == 0)
        {
            if (_activeCharacter == null)
            {
                ActivateNextCharacter();
            }
        }
    }

    void UpdateBattlePhases()
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
        for (int t = 0; t < _NumOfTeams; t++)
        {
            Team team = new Team();
            for (int c = 0; c < _NumOfPlayers; c++)
            {
                GameObject gameObject = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
                gameObject.name = t + "-" + c;
                if (t == 0 && c == 0) gameObject.name = "Col. Ian McCaskill";
                team.Characters.Add(gameObject.GetComponent<Character>());
                gameObject.GetComponent<Character>().Team = team;
                gameObject.GetComponentInChildren<MeshRenderer>().material.color = teamColors[t];
                // register events
                gameObject.GetComponent<Character>().OnActionComplete += HandleCharacter_ActionComplete;
                gameObject.GetComponent<Character>().OnPass += HandleCharacter_Pass;
            }
            _teams.Enqueue(team);
        }
    }

    void StartTeamTurn()
    {
        _teams.Peek().StartTurn();
    }

    void ActivateNextCharacter()
    {
        _activeCharacter = _teams.Peek().GetFirstReadyCharacter();
        while (_activeCharacter == null && _teams.Count > 0)
        {
            Team team = _teams.Dequeue();
            team.EndTurn();
            _teams.Enqueue(team);
            StartTeamTurn();
            _activeCharacter = _teams.Peek().GetFirstReadyCharacter();
            if (_activeCharacter == null)
            {
                _teams.Dequeue();
            }
        }
        if (_teams.Count == 0)
        {
            Application.LoadLevel(Application.loadedLevel);
            return;
        }
        _activeCharacter.Activate();
        OnCharacterActivated(_activeCharacter);
    }

    void HandleCharacter_ActionComplete()
    {
        _activeCharacter = null;
    }

    void HandleCharacter_Pass()
    {
        _activeCharacter = null;
        _teams.Peek().RotateReadyCharacters();
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
        if (_activeCharacter != null)
        {
            foreach (Team team in _teams)
            {
                if (!team.Characters.Contains(_activeCharacter))
                {
                    foreach (Character character in team.Characters)
                    {
                        Ray ray;
                        float rayLength;
                        if (GridCoverManager.Instance.LineOfSight(_activeCharacter.GetComponent<GridEntity>(), character.GetComponent<GridEntity>(), out ray, out rayLength, new List<GridNode[]>()))
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
