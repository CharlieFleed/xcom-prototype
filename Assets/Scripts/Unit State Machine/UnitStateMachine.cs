using UnityEngine;
using System.Collections;
using HSM;
using System.Collections.Generic;
using DecisionTree;

public class UnitStateMachine : MonoBehaviour
{
    HierarchicalStateMachine _hsm;
    public string _states;

    UnitDecisionTree _unitDecisionTree;
    Viewer _viewer;
    Unit _unit;

    private void Awake()
    {
        _unitDecisionTree = GetComponent<UnitDecisionTree>();
        _viewer = GetComponent<Viewer>();
        _unit = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        NetworkMatchManager.Instance.OnTurnBegin += HandleMatchManager_OnTurnBegin;
        Viewer.OnEnemySpotted += HandleViewer_OnEnemySpotted;
    }

    private void OnDisable()
    {
        NetworkMatchManager.Instance.OnTurnBegin -= HandleMatchManager_OnTurnBegin;
        Viewer.OnEnemySpotted -= HandleViewer_OnEnemySpotted;
    }

    private void HandleViewer_OnEnemySpotted(Viewer obj)
    {
        if (obj == _viewer)
            UpdateStateMachine();
    }

    void HandleMatchManager_OnTurnBegin()
    {
        UpdateStateMachine();
    }

    // Use this for initialization
    void Start()
    {

        State stateGuarding = new State(
            "Guarding",
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Guarding-Active"))*/ },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Guarding-Entry")), */new SetDecisionTreeAction(_unitDecisionTree, _unitDecisionTree.GuardingDecisionTree) },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Guarding-Exit"))*/ }
            );
        State statePatrolling = new State(
            "Patrolling",
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Patrolling-Active"))*/ },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Patrolling-Entry")), */new SetDecisionTreeAction(_unitDecisionTree, _unitDecisionTree.PatrollingDecisionTree) },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Patrolling-Exit"))*/ }
            );
        State stateFleeing = new State(
            "Fleeing",
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Fleeing-Active"))*/ },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Fleeing-Entry")), */new SetDecisionTreeAction(_unitDecisionTree, _unitDecisionTree.FleeingDecisionTree) },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Fleeing-Exit"))*/ }
            );
        State stateFighting = new State(
            "Fighting",
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Fighting-Active"))*/ },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Fighting-Entry")), */new SetDecisionTreeAction(_unitDecisionTree, _unitDecisionTree.FightingDecisionTree) },
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Fighting-Exit"))*/ }
            );
        State stateNotEngaged = new HierarchicalStateMachine(
            "Not Engaged",
            new List<State>() { stateGuarding, statePatrolling },
            stateGuarding,
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Not Engaged-Active"))*/ },
            new List<ActionBase>() { new Action(() => Debug.Log($"{name} Not Engaged-Entry")) },
            new List<ActionBase>() { new Action(() => Debug.Log($"{name} Not Engaged-Exit")) }
            );
        State stateEngaged = new HierarchicalStateMachine(
            "Engaged",
            new List<State>() { stateFighting, stateFleeing },
            stateFighting,
            new List<ActionBase>() { /*new Action(() => Debug.Log($"{name} Engaged-Active"))*/ },
            new List<ActionBase>() { new Action(() => Debug.Log($"{name} Engaged-Entry")) },
            new List<ActionBase>() { new Action(() => Debug.Log($"{name} Engaged-Exit")) }
            );
        _hsm = new HierarchicalStateMachine(
            "Top Level HSM",
            new List<State>() { stateNotEngaged, stateEngaged },
            stateNotEngaged,
            new List<ActionBase>(),
            new List<ActionBase>(),
            new List<ActionBase>()
            );

        stateNotEngaged.SetTransitions(new List<Transition>() {
            new Transition(new DoISeeAnEnemyCondition(_viewer), stateEngaged, 0, new List<ActionBase>() {
                new Action(() => Debug.Log($"{name} Transition to {stateEngaged.Name}")),
                new EngageAction(_unit) })
        });
    }    

    void UpdateStateMachine()
    {
        UpdateResult updateResult;
        do
        {
            updateResult = _hsm.Update();
            foreach (var action in updateResult.actions)
            {
                action.Execute();
            }
        }
        while (!updateResult.stable);
        List<State> states = _hsm.GetStates();
        _states = "";
        foreach (var state in states)
        {
            _states += "/" + state.Name;
        }
    }
}
