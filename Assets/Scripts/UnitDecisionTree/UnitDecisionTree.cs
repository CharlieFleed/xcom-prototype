using DecisionTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDecisionTree : MonoBehaviour
{
    DecisionTreeNode _decisionTree;

    List<GridNode> _coverPositions;
    List<GridNode> _reachablePositions;
    List<ShotStats> _shots;

    UnitLocalController _unit;
    GridEntity _gridEntity;
    GridAgent _gridAgent;
    Walker _walker;
    Overwatcher _overwatcher;
    Reloader _reloader;
    Shooter _shooter;
    Health _health;
    Hunkerer _hunkerer;
    ActionsController _actionsController;

    AmIFlankedDecision _amIFlankedDecision;
    DoIHaveAmmoDecision _doIHaveAmmoDecision;
    DoIHaveAGoodShotDecision _doIHaveAGoodShotDecision;
    CanIReachCoverDecision _canIReachCoverDecision;
    AmILowHPDecision _amILowHPDecision;

    OverwatchDecision _overwatchDecision;
    ReloadDecision _reloadDecision;
    MoveToBestCoverDecision _moveToBestCoverDecision;
    FireBestShotDecision _fireBestShotDecision;
    HunkerDecision _hunkerDecision;
    RunFromEnemiesDecision _runFromEnemiesDecision;

    private void Awake()
    {
        _unit = GetComponent<UnitLocalController>();
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
        _walker = GetComponent<Walker>();
        _overwatcher = GetComponent<Overwatcher>();
        _reloader = GetComponent<Reloader>();
        _shooter = GetComponent<Shooter>();
        _health = GetComponent<Health>();
        _hunkerer = GetComponent<Hunkerer>();
        _actionsController = GetComponent<ActionsController>();
        
        _coverPositions = new List<GridNode>();
        _reachablePositions = new List<GridNode>();
        _shots = new List<ShotStats>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _moveToBestCoverDecision = new MoveToBestCoverDecision(_unit, _coverPositions);
        _overwatchDecision = new OverwatchDecision(_overwatcher);
        _reloadDecision = new ReloadDecision(_reloader);
        _fireBestShotDecision = new FireBestShotDecision(_shooter, _shots);
        _hunkerDecision = new HunkerDecision(_hunkerer);
        _runFromEnemiesDecision = new RunFromEnemiesDecision(_unit, _reachablePositions);

        _canIReachCoverDecision = new CanIReachCoverDecision(_unit, _coverPositions, _reachablePositions, _moveToBestCoverDecision, _runFromEnemiesDecision);
        _doIHaveAGoodShotDecision = new DoIHaveAGoodShotDecision(_shooter, _shots, _fireBestShotDecision, _overwatchDecision);
        _doIHaveAmmoDecision = new DoIHaveAmmoDecision(_shooter.Weapon, _doIHaveAGoodShotDecision, _reloadDecision);
        _amIFlankedDecision = new AmIFlankedDecision(_gridAgent, _canIReachCoverDecision, _doIHaveAmmoDecision);
        _amILowHPDecision = new AmILowHPDecision(_health, _hunkerDecision, _overwatchDecision);

        _decisionTree = _amIFlankedDecision;
    }

    public void Run()
    {
        ((FinalDecision)_decisionTree.MakeDecision()).Execute();
    }
}