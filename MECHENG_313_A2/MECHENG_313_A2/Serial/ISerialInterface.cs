using MECHENG_313_A2.Views;
using System;
using System.Threading.Tasks;

namespace MECHENG_313_A2.Serial
{
    public delegate void SerialReadAction(DateTime timestamp, string serialInput);

    internal interface ISerialInterface
    {
        string PortOpened { get; }

        /// <summary>
        /// Get an array of available port names.
        /// </summary>
        /// <returns>A string array of available port names.</returns>
        Task<string[]> GetPortNames();

        /// <summary>
        /// Open the specified serial port at a specified baud rate.
        /// </summary>
        /// <returns>True if the port was successfully opened, false otherwise.</returns>
        Task<bool> OpenPort(string portName, int baudRate);

        /// <summary>
        /// Set the state of the traffic light on the microcontroller via the serial interface.
        /// </summary>
        /// <returns>Serial response from the microcontroller.</returns>
        Task<string> SetState(TrafficLightState state);
    }
}
