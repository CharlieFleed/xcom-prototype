using System.Collections;
using System.Collections.Generic;
using DecisionTree;
using UnityEngine;

namespace HSM
{
    public class Transition
    {
        State _targetState;
        List<ActionBase> _actions;
        int _level;
        ConditionBase _condition;

        public Transition(ConditionBase condition, State targetState, int level, List<ActionBase> actions)
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
        public virtual List<ActionBase> GetActions()
        {
            return _actions;
        }

        public virtual bool IsTriggered()
        {
            return _condition.Test();
        }
    }    
}