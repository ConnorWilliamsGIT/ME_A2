using MECHENG_313_A2.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MECHENG_313_A2.Tasks
{
    /// <summary>
    /// Enum representing the task number.
    /// </summary>
    public enum TaskNumber
    {
        Task2,
        Task3
    }

    public interface IController
    {
        /// <summary>
        /// Gets the task number.
        /// </summary>
        TaskNumber TaskNumber { get; }

        /// <summary>
        /// Configures the length of the red and green lights (in ms), only used in Task 3.
        /// </summary>
        void ConfigLightLength(int redLength, int greenLength);

        /// <summary>
        /// This would be called by the click of Config button, representing a config event.
        /// 
        /// <para>
        /// For Task 2, configuration mode should be entered only when the red light is on. 
        /// This should return true or false immediately based on the state of the traffic light.
        /// </para>
        /// 
        /// <para>
        /// For Task 3, this should wait until the red light ends, then enter configuration mode and return true.
        /// </para>
        /// </summary>
        /// <returns>Whether the app has entered configuration mode.</returns>
        Task<bool> EnterConfigMode();

        /// <summary>
        /// This would be called by the click of Exit config mode button, representing another config event.
        /// In both task 2 and 3, this should make the finite state machines exit configuration mode immediately.
        /// </summary>
        void ExitConfigMode();

        /// <summary>
        /// Gets an array of available serial port names. Use MockSerialInterface or your own implementation of 
        /// ISerialInterface to get the list of available ports.
        /// This will be called by the click of Detect Serial Ports button.
        /// </summary>
        /// <returns>An array of available serial port names.</returns>
        Task<string[]> GetPortNames();

        /// <summary>
        /// <para>
        /// Opens a log file for writing and populate the GUI with existing log entries in the file (optional, 
        /// this is a design decision, you can choose to create a new log file every time). 
        /// This will be called by the click of Open Log File button.
        /// </para>
        /// 
        /// <para>
        /// It is up to you to decide what to do with the log file(s) and the format of the log entries. However, 
        /// due to strict file access control, you may only be able to write to files under certain directories (e.g. the Environment.SpecialFolder.LocalApplicationData). 
        /// Check <see href="https://learn.microsoft.com/en-us/xamarin/xamarin-forms/data-cloud/data/files?tabs=windows"/> and 
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.file?view=netstandard-2.0"/> for more details on how to properly create/read/write files.
        /// </para>
        /// 
        /// <para>
        /// Use TaskPage.SetLogEntries to populate the GUI with existing log entries. When there are a lot of entries, 
        /// iteratively call AddLogEntry may result in severe performance issues.
        /// </para>
        /// </summary>
        /// <returns>The full path of the opened log file.</returns>
        Task<string> OpenLogFile();

        /// <summary>
        /// Opens a serial port. Again, use MockSerialInterface or your own implementation of ISerialInterface.
        /// </summary>
        /// <returns>True if the serial port is successfully opened; otherwise, false.</returns>
        Task<bool> OpenPort(string serialPort, int baudRate);

        /// <summary>
        /// Registers the task page, so the controller can call methods on the task page.
        /// </summary>
        void RegisterTaskPage(ITaskPage taskPage);

        /// <summary>
        /// Starts the traffic light system. This will be called by the click of Start button.
        /// 
        /// <para>
        /// For both task 2 and 3, the traffic light system should start with the green light on.
        /// </para>
        /// </summary>
        void Start();

        /// <summary>
        /// Triggers a tick event. 
        /// In task 2, this will be called by the click of Tick button.
        /// In tast 3, however, this should be triggered automatically after a defined interval 
        /// (e.g. 1000 ms for Yellow -> Red light, etc.).
        /// </summary>
        void Tick();
    }
}
