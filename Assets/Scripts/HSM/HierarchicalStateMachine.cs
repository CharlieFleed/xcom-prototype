using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    public class HierarchicalStateMachine : State
    {
        /// <summary>
        /// List of states at this level of the hierarchy.
        /// </summary>
        List<State> _states = new List<State>();
        /// <summary>
        /// The initial state for when the machine has no current state.
        /// </summary>
        public State _initialState;
        public State _currentState;

        public HierarchicalStateMachine(string name, List<State> states, State initialState, List<Action> actions, List<Action> entryActions, List<Action> exitActions) : base(name, actions, entryActions, exitActions)
        {
            _states = states;
            _initialState = initialState;
        }

        public override List<State> GetStates()
        {
            if (_currentState != null)
            {
                return _currentState.GetStates();
            }
            else
            {
                return new List<State>();
            }
        }

        /// <summary>
        /// Recursively updates the machine.
        /// </summary>
        /// <returns></returns>
        public override UpdateResult Update()
        {
            UpdateResult result = new UpdateResult();

            // If we are in no state, use the initial state
            if (_currentState == null)
            {
                _currentState = _initialState;
                result.actions = _currentState.GetEntryActions();
                return result;
                // we don't check transitions from this state until the next update
            }

            Transition triggeredTransition = null;
            // Try to find a transition in the current state
            foreach (var transition in _currentState.GetTransitions())
            {
                if (transition.IsTriggered())
                {
                    triggeredTransition = transition;
                    break;
                }
            }

            // If we have found one, make a result structure for it
            if (triggeredTransition != null)
            {
                result.transition = triggeredTransition;
                result.transitionLevel = triggeredTransition.GetLevel();
            }
            // Otherwise recurse down for a result
            else
            {
                result = _currentState.Update();                
            }

            // Check if the result contains a transition
            if (result.transition != null)
            {
                // Act based on its level
                if (result.transitionLevel == 0)
                {
                    // It's on our level, honor it
                    State targetState = result.transition.GetTargetState();
                    result.actions.AddRange(_currentState.GetExitActions());
                    result.actions.AddRange(result.transition.GetActions());
                    result.actions.AddRange(targetState.GetEntryActions());

                    // Set our new current state
                    _currentState = targetState;

                    // Add our normal action (we may be a state and not the top level machine)
                    result.actions.AddRange(GetActions());

                    // Clear the transition, so nobody else does it
                    result.transition = null;
                }
                else if (result.transitionLevel > 0)
                {
                    // It's destined for a higher level
                    // Exit our current state
                    result.actions.AddRange(_currentState.GetExitActions());
                    _currentState = null;

                    // Decrease the number of levels to go
                    result.transitionLevel -= 1;
                }
                else
                {
                    // It needs to be passed down
                    State targetState = result.transition.GetTargetState();
                    HierarchicalStateMachine targetMachine = targetState._parent;
                    result.actions.AddRange(result.transition.GetActions());
                    // UpdateDown eventually will change our current state to the ancestor of target state, pass the level of the transition changed into a positive number
                    result.actions.AddRange(targetMachine.UpdateDown(targetState, -result.transitionLevel));

                    // Clear the transition, so nobody else does it
                    result.transition = null;
                }
            }
            // If we didn't get a transition
            else
            {
                // We can simply do our normal action
                result.actions.AddRange(GetActions());
            }

            return result;
        }

        /// <summary>
        /// Recurses up the parent hierarchy, transitioning into each state in turn for the given number of levels
        /// </summary>
        /// <param name="state"></param>
        /// <param name="levelsToGo"></param>
        /// <returns></returns>
        public List<Action> UpdateDown(State state, int levelsToGo)
        {
            List<Action> actions = new List<Action>();

            // If we are not at top level, we have a parent
            if (levelsToGo > 0)
            {
                // Pass ourself as the transition state to our parent
                actions.AddRange(_parent.UpdateDown(this, levelsToGo - 1));
            }
            // Otherwise we have no actions to add to
            else
            {
                // we are the ancestor
            }

            // If we have a current state, exit it
            if (_currentState != null)
            {
                actions.AddRange(_currentState.GetExitActions());
            }

            // Move to the new state, and return all the actions
            _currentState = state;
            actions.AddRange(state.GetEntryActions());

            return actions;
        }
    }
}
