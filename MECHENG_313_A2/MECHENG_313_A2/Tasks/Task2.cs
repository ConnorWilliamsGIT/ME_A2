using MECHENG_313_A2.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MECHENG_313_A2.Serial;

namespace MECHENG_313_A2.Tasks
{
    
    
    internal class Task2 : IController
    {
        
        
        private MockSerialInterface serialInterface = new MockSerialInterface();
        private List<string> states = new List<string> {"red", "green", "yellow", "configYellow", "configBlack"};
        private List<string> events = new List<string> {"tick", "config", "tick-c"};
        private bool configRequested = false;
        private FiniteStateMachine fsm = new FiniteStateMachine();
        
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
        }
        private void GoToRed(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.Red);
        }

        private void GoToYellow(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.Yellow);
        }

        private void GoToBlack(DateTime timeStamp)
        {
            _taskPage.SetTrafficLightState(TrafficLightState.None);
        }
        
        public void ConfigLightLength(int redLength, int greenLength)
        {
            // TODO: Implement this
        }

        public async Task<bool> EnterConfigMode()
        {
            try
            { 
                configRequested = fsm.ProcessEvent(events[1]) != states[3];
            }
            catch (Exception e)
            {
                // todo log error
                return false;
            }

            return true;
        }

        public void ExitConfigMode()
        {
            configRequested = false;
            if (fsm.GetCurrentState() == states[0])
            {
                return;
            }
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
            // }
            // string text = File.ReadAllText(filePath);
            // return text ?? "log file empty";
        }

        public async Task<bool> OpenPort(string serialPort, int baudRate)
        {
            try
            {
                serialInterface.OpenPort(serialPort, baudRate);
            }
            catch (Exception e)
            {
                //todo add log for this
                return false;
            }
            return true;
        }

        public void RegisterTaskPage(ITaskPage taskPage)
        {
            _taskPage = taskPage;
        }

        public void Start()
        {
            fsm.SetCurrentState(states[1]);
            _taskPage.SetTrafficLightState(TrafficLightState.Green);
        }

        public void Tick()
        {
            if (fsm.ProcessEvent(configRequested ? "tick-c" : "tick") == states[3])
            {
                configRequested = false;
            };
        }
    }
}
