using MECHENG_313_A2.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MECHENG_313_A2.Serial
{
    internal class ArduinoSerialInterface : ISerialInterface
    {
        private readonly ISerialPort _serialPort;
        private string _portName = null;
        volatile private string _serialDataReceived = null;

        public string PortOpened => _portName;

        public ArduinoSerialInterface()
        {
            _serialPort = DependencyService.Get<ISerialPort>();
            _serialPort.DataReceived += OnDataReceived;
        }

        public async Task<string[]> GetPortNames()
        {
            return await _serialPort.GetPortNames();
        }

        public async Task<bool> OpenPort(string portName, int baudRate)
        {
            if (_serialPort == null)
            {
                throw new InvalidOperationException("Serial port not supported on this platform.");
            }

            if (await _serialPort.Open(portName, baudRate))
            {
                _portName = portName;
                return true;
            }

            return false;
        }

        public async Task<string> SetState(TrafficLightState state)
        {
            if (_serialPort == null || !await _serialPort.Write($"{state.ToString().ToUpper()}\n"))
            {
                return null;
            }

            return await Task.Run(() =>
            {
                while (_serialDataReceived is null) { }
                string tmp = _serialDataReceived;
                _serialDataReceived = null;

                return tmp;
            });
        }

        public bool ClosePort()
        {
            if (_serialPort == null)
            {
                return false;
            }

            _serialPort.Close();

            return true;
        }

        private void OnDataReceived(object sender, string data)
        {
            if (Enum.TryParse<TrafficLightState>(data.Trim(' ', '\n'), true, out var state))
            {
                _serialDataReceived = data.Trim(' ', '\n');
            }
            else
            {
                _serialDataReceived = "ERROR";
            }
        }
    }
}
