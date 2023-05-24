using MECHENG_313_A2.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MECHENG_313_A2.Serial
{
    internal class MockSerialInterface : ISerialInterface
    {
        private string _portName;
        private string[] _availablePorts;

        public string PortOpened => _portName;

        public MockSerialInterface()
        {
            _availablePorts = new string[] { "COM1", "COM2", "COM3" };
        }

        /// <summary>
        /// Get an array of mocked port names.
        /// </summary>
        /// <returns>{ "COM1", "COM2", "COM3" }.</returns>
        public async Task<string[]> GetPortNames()
        {
            return _availablePorts;
        }

        /// <summary>
        /// Open the specified serial port at a specified baud rate.
        /// For this mocked serial interface, it will accept any port name belonging to { "COM1", "COM2", "COM3" }.
        /// False will be returned if the port is already opened.
        /// </summary>
        /// <returns>True if the port was successfully opened, false otherwise.</returns>
        public async Task<bool> OpenPort(string portName, int baudRate)
        {
            if (_portName == portName) return false;

            _portName = portName;
            return _availablePorts.Contains(portName);
        }

        /// <summary>
        /// Set the state of the traffic light via the serial interface.
        /// This mocked serial interface will simulate a delay of 10ms before returning the response from the Arduino board.
        /// </summary>
        /// <returns>Mocked serial response from the Arduino board</returns>
        public async Task<string> SetState(TrafficLightState state)
        {
            await Task.Delay(10); // simulate a delay from Arduino board serial response
            return state.ToString().ToUpper();
        }
    }
}
