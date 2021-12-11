using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    public class State
    {
        public string Name => _name; 

        List<ActionBase> _actions = new List<ActionBase>();
        List<ActionBase> _entryActions = new List<ActionBase>();
        List<ActionBase> _exitActions = new List<ActionBase>();
        List<Transition> _transitions = new List<Transition>();
        protected string _name;

        public HierarchicalStateMachine _parent;

        public State(string name, List<ActionBase> actions, List<ActionBase> entryActions, List<ActionBase> exitActions)
        {
            _name = name;
            _actions = actions;
            _entryActions = entryActions;
            _exitActions = exitActions;
        }

        public virtual List<State> GetStates()
        {
            return new List<State>() { };
        }        

        public virtual UpdateResult Update()
        {
            UpdateResult result = new UpdateResult();
            result.actions.AddRange(GetActions());
            return result;
        }

        public List<ActionBase> GetActions()
        {
            return _actions;
        }
        public List<ActionBase> GetEntryActions()
        {
            return _entryActions;
        }
        public List<ActionBase> GetExitActions()
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
