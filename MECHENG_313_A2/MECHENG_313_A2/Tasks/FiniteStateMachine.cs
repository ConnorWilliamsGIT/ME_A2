using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MECHENG_313_A2.Tasks
{
    public class FiniteStateMachine : IFiniteStateMachine
    {
        //table stores the next action and delegate to the action method to take during transition
        private Dictionary<string, Dictionary<string, (TimestampedAction action, string nextState)>> fsTable;

        private string currentState;

        //define constructor
        public FiniteStateMachine() // string startingState
        {
            //set current state and create the table
            // currentState = startingState;
            fsTable = new Dictionary<string, Dictionary<string, (TimestampedAction action, string nextState)>>();
        }

        public void addState(string state)
        {
            //check if the state already exists
            if (fsTable.ContainsKey(state) == false)
            {
                //if not create the new state with a blank dictionary of actions
                fsTable.Add(state, new Dictionary<string, (TimestampedAction action, string nextState)>());
            }
        }

        public void AddAction(string state, string eventTrigger, TimestampedAction action)
        {
            //check if the state exists
            if (fsTable.ContainsKey(state) == false)
            {
                return;
            }

            //check if the action already exists on the state
            if (fsTable[state].ContainsKey(eventTrigger))
            {
                //if it does then add the action and keep the current "next state"
                fsTable[state][eventTrigger] = (action, fsTable[state][eventTrigger].nextState);
            }
            else
            {
                //else create the new action with a null next state
                fsTable[state].Add(eventTrigger, (action, null));
            }
        }

        public void createEvent(string state, string eventTrigger, TimestampedAction action, string nextState)
        {
            //check if the state exists
            if (fsTable.ContainsKey(state) == false)
            {
                return;
            }

            //check if the action already exists on the state
            if (fsTable[state].ContainsKey(eventTrigger))
            {
                //if it does then add the action and keep the current "next state"
                fsTable[state][eventTrigger] = (action, nextState);
            }
            else
            {
                //else create the new action with a null next state
                fsTable[state].Add(eventTrigger, (action, nextState));
            }
        }

        public string GetCurrentState()
        {
            return currentState;
        }

        public string ProcessEvent(string eventTrigger)
        {
            //check if the event trigger exists and if the next state is not null
            if (!fsTable[currentState].ContainsKey(eventTrigger)) return currentState;
            if (fsTable[currentState][eventTrigger].nextState == null) return currentState;
            string current = currentState;
            //start the thread to run the action
            Thread actionThread = new Thread(new ThreadStart(() =>
            {
                lock (current)
                {
                    fsTable[current][eventTrigger].action(DateTime.Now);
                }
            }));
            actionThread.Start();
            currentState = fsTable[currentState][eventTrigger].nextState ?? currentState;
            //return the next state if it exists or the current state if it doesnt
            return currentState;
        }

        public void SetCurrentState(string state)
        {
            currentState = state;
        }

        public void SetNextState(string state, string nextState, string eventTrigger)
        {
            //check if the state exists
            if (fsTable.ContainsKey(state) == false)
            {
                return;
            }

            if (fsTable[state].ContainsKey(eventTrigger))
            {
                //if it does then add the action and keep the current "next state"
                fsTable[state][eventTrigger] = (fsTable[state][eventTrigger].action, nextState);
            }
            else
            {
                //else create the new action with a null next state
                fsTable[state].Add(eventTrigger, (null, nextState));
            }
        }
    }
}