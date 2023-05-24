using System;
using System.Collections.Generic;
using System.Text;

namespace MECHENG_313_A2.Views
{
    public interface ITaskPage
    {
        /// <summary>
        /// Sets the traffic light state. This will update the traffic light image.
        /// </summary>
        /// <param name="state">The new state of the traffic light.</param>
        void SetTrafficLightState(TrafficLightState state);

        /// <summary>
        /// Adds a log entry to the view model. This will update the GUI log entries.
        /// </summary>
        /// <param name="logEntry">The log entry to be added to the view model</param>
        void AddLogEntry(string logEntry);

        /// <summary>
        /// Sets the log entries in the view model. This will overwrite the entire GUI log entries.
        /// Each string element in the array corresponds to one log entry.
        /// The first array element should reflect the newest log entry.
        /// </summary>
        /// <param name="logEntries">The log entries to set.</param>
        void SetLogEntries(string[] logEntries);

        /// <summary>
        /// Writes the given serial input to the Serial Monitor UI element.
        /// </summary>
        /// <param name="timestamp">The timestamp of the serial input.</param>
        /// <param name="input">The serial input to write.</param>
        void SerialPrint(DateTime timestamp, string input);

        /// <summary>
        /// Displays an alert window with the given title and message.
        /// </summary>
        /// <param name="title">The title of the alert window.</param>
        /// <param name="message">The message to display in the alert window.</param>
        /// <param name="cancel">The text to display in the cancel button.</param>
        void ShowAlert(string title, string message, string cancel);
    }
}
