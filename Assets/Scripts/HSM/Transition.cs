using System.Collections;
using System.Collections.Generic;
using DecisionTree;
using UnityEngine;

namespace HSM
{
    public class Transition
    {
        State _targetState;
        List<Action> _actions;
        int _level;
        Condition _condition;

        public Transition(Condition condition, State targetState, int level, List<Action> actions)
        {
            _condition = condition;
            _targetState = targetState;
            _level = level;
            _actions = actions;
        }

        /// <summary>
        /// Return the difference in levels of the hierarchy from the source to the target of the transition
        /// </summary>
        /// <returns></returns>
        public virtual int GetLevel()
        {
            return _level;
        }
        public virtual State GetTargetState()
        {
            return _targetState;
        }
        public virtual List<Action> GetActions()
        {
            return _actions;
        }

        public virtual bool IsTriggered()
        {
            return _condition.Test();
        }
    }    
}