using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    public class State
    {
        List<Action> _actions = new List<Action>();
        List<Action> _entryActions = new List<Action>();
        List<Action> _exitActions = new List<Action>();
        List<Transition> _transitions = new List<Transition>();
        protected string _name;

        public HierarchicalStateMachine _parent;

        public State(string name, List<Action> actions, List<Action> entryActions, List<Action> exitActions)
        {
            _name = name;
            _actions = actions;
            _entryActions = entryActions;
            _exitActions = exitActions;
        }

        public virtual List<State> GetStates()
        {
            return new List<State>() { this };
        }
        

        public virtual UpdateResult Update()
        {
            UpdateResult result = new UpdateResult();
            result.actions.AddRange(GetActions());
            return result;
        }

        public List<Action> GetActions()
        {
            return _actions;
        }
        public List<Action> GetEntryActions()
        {
            return _entryActions;
        }
        public List<Action> GetExitActions()
        {
            return _exitActions;
        }

        public List<Transition> GetTransitions()
        {
            return _transitions;
        }

        public void SetTransitions(List<Transition> transitions)
        {
            _transitions = transitions;
        }
    }
}
