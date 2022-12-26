using DecisionTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDecisionTree : MonoBehaviour
{
    DecisionTreeNode _decisionTree;

    public DecisionTreeNode FightingDecisionTree;
    public DecisionTreeNode FleeingDecisionTree; // TODO: not implemented
    public DecisionTreeNode GuardingDecisionTree;
    public DecisionTreeNode PatrollingDecisionTree; // TODO: not implemented

    List<GridNode> _coverPositions;
    List<GridNode> _reachablePositions;
    List<ShotStats> _shots;

    Unit _unit;
    GridEntity _gridEntity;
    GridAgent _gridAgent;
    Walker _walker;
    Overwatcher _overwatcher;
    Reloader _reloader;
    Shooter _shooter;
    Health _health;
    Hunkerer _hunkerer;
    Skipper _skipper;
    ActionsController _actionsController;

    DecisionAmIFlanked _amIFlankedDecision;
    DecisionDoIHaveAmmo _doIHaveAmmoDecision;
    DecisionDoIHaveAGoodShot _doIHaveAGoodShotDecision;
    DecisionCanIReachCover _canIReachCoverDecision;
    DecisionAmILowHP _amILowHPDecision;

    FinalDecisionOverwatch _overwatchDecision;
    FinalDecisionReload _reloadDecision;
    FinalDecisionMoveToBestCover _moveToBestCoverDecision;
    FinalDecisionFireBestShot _fireBestShotDecision;
    FinalDecisionHunker _hunkerDecision;
    FinalDecisionRunFromEnemies _runFromEnemiesDecision;

    DecisionCanIReachCover _canIReachCoverDecision2;
    DecisionCanIReachBetterCover _canIReachBetterCoverDecision;

    FinalDecisionSkip _skipDecision;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _gridEntity = GetComponent<GridEntity>();
        _gridAgent = GetComponent<GridAgent>();
        _walker = GetComponent<Walker>();
        _overwatcher = GetComponent<Overwatcher>();
        _reloader = GetComponent<Reloader>();
        _shooter = GetComponent<Shooter>();
        _health = GetComponent<Health>();
        _hunkerer = GetComponent<Hunkerer>();
        _skipper = GetComponent<Skipper>();
        _actionsController = GetComponent<ActionsController>();
        
        _coverPositions = new List<GridNode>();
        _reachablePositions = new List<GridNode>();
        _shots = new List<ShotStats>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Fighting Decision Tree
        _moveToBestCoverDecision = new FinalDecisionMoveToBestCover(_unit, _coverPositions);
        _overwatchDecision = new FinalDecisionOverwatch(_overwatcher);
        _reloadDecision = new FinalDecisionReload(_reloader);
        _fireBestShotDecision = new FinalDecisionFireBestShot(_shooter, _shots);
        _hunkerDecision = new FinalDecisionHunker(_hunkerer);
        _runFromEnemiesDecision = new FinalDecisionRunFromEnemies(_unit, _reachablePositions);

        _canIReachBetterCoverDecision = new DecisionCanIReachBetterCover(_unit, _coverPositions, _moveToBestCoverDecision, _overwatchDecision);
        _canIReachCoverDecision2 = new DecisionCanIReachCover(_unit, _coverPositions, _reachablePositions, _canIReachBetterCoverDecision, _overwatchDecision);
        _canIReachCoverDecision = new DecisionCanIReachCover(_unit, _coverPositions, _reachablePositions, _moveToBestCoverDecision, _runFromEnemiesDecision);
        _amILowHPDecision = new DecisionAmILowHP(_health, _hunkerDecision, _canIReachCoverDecision2);
        _doIHaveAGoodShotDecision = new DecisionDoIHaveAGoodShot(_shooter, _shots, _fireBestShotDecision, _amILowHPDecision);
        _doIHaveAmmoDecision = new DecisionDoIHaveAmmo(_shooter != null ?_shooter.Weapon :null, _doIHaveAGoodShotDecision, _reloadDecision); // account for units without a shooter
        _amIFlankedDecision = new DecisionAmIFlanked(_gridAgent, _canIReachCoverDecision, _doIHaveAmmoDecision);

        FightingDecisionTree = _amIFlankedDecision;

        // Guarding Decision Tree 
        _skipDecision = new FinalDecisionSkip(_skipper);

        GuardingDecisionTree = _skipDecision;

        // Initialisation
        SetDecisionTree(FightingDecisionTree);
    }

    public void Run()
    {
        ((FinalDecision)_decisionTree.MakeDecision()).Execute();
    }

    public void SetDecisionTree(DecisionTreeNode decisionTreeNode)
    {
        _decisionTree = decisionTreeNode;
    }
}
