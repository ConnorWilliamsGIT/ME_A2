using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using MECHENG_313_A2.Views;

namespace MECHENG_313_A2.Tasks
{
    internal class Task3 : Task2
    {
        private Timer timer;
        public override TaskNumber TaskNumber => TaskNumber.Task3;
        
        public override void Start()
        {
            //set up the starting state
            fsm.SetCurrentState(states[1]);
            _taskPage.SetTrafficLightState(TrafficLightState.Green);
            SetSerialState(TrafficLightState.Green);
            //use a sperate thread and timer to run the tick method
            timer = new Timer(lightLength[0]);
            timer.Elapsed += TimerElapsed;
            timer.AutoReset = true;
            timer.Enabled = true;
            //log that the traffic light has started and the time it started
            log("Traffic light started");
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Call the Tick method here
            Tick();
            //update the timer with the new intervals
            string state = fsm.GetCurrentState();
            if (state == "green")
            {
                timer.Interval = lightLength[0];
            }else if (state == "red")
            {
                timer.Interval = lightLength[1];
            }
            else
            {
                timer.Interval = 1000;
            }
        }
    }
    
 
    
 
}
