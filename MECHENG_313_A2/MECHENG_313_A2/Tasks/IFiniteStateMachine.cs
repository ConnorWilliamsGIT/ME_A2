using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MECHENG_313_A2.Tasks
{
    public delegate void TimestampedAction(DateTime timestamp);

    internal interface IFiniteStateMachine
    {
        /// <summary>
        /// Sets the next state for a given state and event trigger.
        /// </summary>
        void SetNextState(string state, string nextState, string eventTrigger);

        /// <summary>
        /// Adds an action to be performed when an event is triggered in a given state.
        /// </summary>
        void AddAction(string state, string eventTrigger, TimestampedAction action);

        /// <summary>
        /// Gets the current state of the finite state machine.
        /// </summary>
        /// <returns>The current state of the finite state machine.</returns>
        string GetCurrentState();

        /// <summary>
        /// Sets the current state of the finite state machine.
        /// </summary>
        void SetCurrentState(string state);

        /// <summary>
        /// Processes an event trigger, perform the actions associated in parallel (multi-threading, 
        /// note that different actions may have different priorities), and return the next state to transit 
        /// into (it is up to you to decide if the transition transition should or should not be performed within 
        /// this method).
        /// </summary>
        /// <returns>The name of the next state, or null otherwise.</returns>
        string ProcessEvent(string eventTrigger);
    }
}
