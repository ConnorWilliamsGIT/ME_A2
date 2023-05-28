using MECHENG_313_A2.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MECHENG_313_A2.Serial;

namespace MECHENG_313_A2.Tasks
{
    
    
    internal class Task2 : IController
    {
        
        
        private MockSerialInterface serialInterface = new MockSerialInterface();
        protected List<string> states = new List<string> {"red", "green", "yellow", "configYellow", "configBlack"};
        protected List<string> events = new List<string> {"tick", "config", "tick-c"};
        private bool configRequested = false;
        protected FiniteStateMachine fsm = new FiniteStateMachine();
        protected int[] lightLength = new[] { 1000, 1000 };
        public Task2()
        {
            foreach (string state in states)
            {
                fsm.addState(state);
            }

            fsm.createEvent(states[0], events[0], GoToGreen, states[1]); // Tick : red to green
            fsm.createEvent(states[1], events[0], GoToYellow, states[2]); // Tick :green to yellow
            fsm.createEvent(states[2], events[0], GoToRed, states[0]); // Tick :yellow to red
            fsm.createEvent(states[3], events[0], GoToBlack, states[4]); // Tick : configYellow to configBlack
            fsm.createEvent(states[4], events[0], GoToYellow, states[3]); // Tick : configBlack to configYellow
            fsm.createEvent(states[0], events[1], GoToYellow, states[3]); // Config : red to configYellow
            fsm.createEvent(states[3], events[1], GoToRed, states[0]); // Config : configYellow to red
            fsm.createEvent(states[4], events[1], GoToRed, states[0]); // Config : configBlack to red
            fsm.createEvent(states[0], events[2], GoToYellow, states[3]); // Tick-c : red to configYellow
            fsm.createEvent(states[1], events[2], GoToYellow, states[2]); // Tick-c : green to yellow
            fsm.createEvent(states[2], events[2], GoToRed, states[0]); // Tick-c : yellow to red
        }

        public virtual TaskNumber TaskNumber => TaskNumber.Task2;

        protected ITaskPage _taskPage;

        private void GoToGreen(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.Green);
            SetSerialState(TrafficLightState.Green);
            log("Traffic light changed to green");
        }
        private void GoToRed(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.Red);
            SetSerialState(TrafficLightState.Red);
            log("Traffic light changed to red");
        }

        private void GoToYellow(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.Yellow);
            SetSerialState(TrafficLightState.Yellow);
            log("Traffic light changed to yellow");
        }

        private void GoToBlack(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.None);
            SetSerialState(TrafficLightState.None);
            log("Traffic light changed to black");
        }
        
        public void ConfigLightLength(int redLength, int greenLength)
        {
            lightLength[0] = (greenLength > 0) ? greenLength : 1000;
            lightLength[1] = (redLength > 0) ? redLength : 1000;
        }
        public int GetLightLength(TrafficLightState state)
        {
            //if its red or green return the length of the light
            if (state == TrafficLightState.Red || state == TrafficLightState.Green)
            {
                return lightLength[(int)state];
            }
            return 1000;
        }
        public async Task<bool> EnterConfigMode()
        {
            try
            { 
                if(fsm.GetCurrentState() == states[3] || fsm.GetCurrentState() == states[4])
                {
                    log("Already in config mode");
                    return true;
                }
                if (fsm.ProcessEvent(events[1]) != states[3])
                {
                    configRequested = true;
                    log("Config mode requested");
                }else
                {
                    log("Entered config mode");
                }
            }
            catch (Exception e)
            {
                log("Error entering config mode: " + e.Message);
                return false;
            }

            return true;
        }

        public void ExitConfigMode()
        {
            if (fsm.GetCurrentState() == states[0])
            {
                log("already in normal mode");
                return;
            }
            log(configRequested ? "No longer waiting for config" : "Config mode exited");
            configRequested = false;
            fsm.ProcessEvent(events[1]);
        }

        public async Task<string[]> GetPortNames()
        {
            return await serialInterface.GetPortNames();
        }

        public async Task<string> OpenLogFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "log.txt");
            // if(!File.Exists(filePath))
            // {
            //     File.Create(filePath);
            //     log("created log file");
            // }
            // string text = File.ReadAllText(filePath);
            // return text ?? "log file empty";
        }

        public async Task<bool> OpenPort(string serialPort, int baudRate)
        {
            try
            {
                serialInterface.OpenPort(serialPort, baudRate);
                log("Opened port " + serialPort + " at " + baudRate.ToString() + " baud");
            }
            catch (Exception e)
            {
                log("Error opening port: " + e.Message);
                return false;
            }
            return true;
        }

        public void RegisterTaskPage(ITaskPage taskPage)
        {
            _taskPage = taskPage;
        }

        public virtual void Start()
        {
            fsm.SetCurrentState(states[1]);
            _taskPage.SetTrafficLightState(TrafficLightState.Green);
            SetSerialState(TrafficLightState.Green);
            //log that the traffic light has started and the time it started
            log("Traffic light started");
        }
        

        public void Tick()
        {
            string tempState = fsm.ProcessEvent(configRequested ? "tick-c" : "tick"); 
            if (tempState == states[3] && configRequested)
            {
                log("entered config mode");
                configRequested = false;
            }
            else
            {
                log("entered state: " + tempState);
            }
        }
        
        protected void log(string message)
        {
            Thread actionThread = new Thread(new ThreadStart(() =>
            {
                lock (_taskPage)
                {
                    _taskPage.AddLogEntry($"{DateTime.Now.ToString():yyyy-MM-dd HH:mm:ss}: {message}");
                }
            }));
            actionThread.Start();
        }
        
        protected async void SetSerialState(TrafficLightState state)
        {
            _taskPage.SerialPrint(DateTime.Now, await serialInterface.SetState(state) + "\n");
        }
    }
}
