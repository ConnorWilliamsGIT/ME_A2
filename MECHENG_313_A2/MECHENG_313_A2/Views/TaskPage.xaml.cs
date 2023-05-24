using MECHENG_313_A2.Tasks;
using MECHENG_313_A2.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MECHENG_313_A2.Views
{

    // Enum for the traffic light states
    public enum TrafficLightState
    {
        Red,
        Green,
        Yellow,
        None
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskPage : ContentPage, ITaskPage
    {
        public static int DEFAULT_GREEN_LIGHT_LENGTH = 1000;
        public static int DEFAULT_YELLOW_LIGHT_LENGTH = 1000;
        public static int DEFAULT_RED_LIGHT_LENGTH = 1000;
        public static int DEFAULT_CONFIG_LIGHT_LENGTH = 1000;

        private IController _controller;
        private TaskViewModel _viewModel;

        private ObservableCollection<string> _serialPorts = new ObservableCollection<string>();
        private string _serialPort = null;
        private ObservableCollection<string> _logEntries = new ObservableCollection<string>();

        private bool _configMode = false;
        private string _logFilePath = null;
        private bool _portOpened = false;

        public TaskPage(IController controller)
        {
            _controller = controller;
            _viewModel = new TaskViewModel();
            InitializeElements();
            _controller.RegisterTaskPage(this);
        }

        private void InitializeElements()
        {
            InitializeComponent();
            BindingContext = _viewModel;

            ConfigButton.IsEnabled = false;
            TickButton.IsEnabled = false;
            _configMode = false;
            SerialOpenButton.IsEnabled = false;
            SerialPortPicker.ItemsSource = _serialPorts;
            DisableConfigPanel();
        }

        // Event handlers
        /// <summary>
        /// Handles the click event of the TickButton.
        /// 
        /// <para>Checks PortOpened and LogFileOpened() through the controller.</para>
        ///
        /// <para>Calls Tick method of the controller.</para>
        /// </summary>
        private async void OnTickButtonClicked(object sender, EventArgs e)
        {
            if (await CheckSetup())
            {
                _controller.Tick();
            }
        }

        /// <summary>
        /// Handles the click event of the ConfigButton.
        /// 
        /// <para>If not already in config mode, this checks PortOpened and LogFileOpened() through the controller, 
        /// and calls EnterConfigMode() method of the controller.</para>
        /// 
        /// <para>If already in config mode, this calls ExitConfigMode() method of the controller and exits config mode.</para>
        /// 
        /// </summary>
        private async void OnConfigButtonClicked(object sender, EventArgs e)
        {
            if (!_configMode && await CheckSetup())
            {
                // Ensure UI updates are done on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConfigButton.Text = "Waiting...";
                    ConfigButton.IsEnabled = false;
                });

                Thread t = new Thread(new ThreadStart(async () =>
                {
                    // Since this will take a long time in Task 3, we will not run this on the UI thread
                    bool success = await _controller.EnterConfigMode();
                    ConfigModeEntered(success);
                }));
                t.Start();
            }
            else if (_configMode)
            {
                _controller.ExitConfigMode();
                UIExitConfigMode();
            }
        }

        /// <summary>
        /// Handles the click event of the SaveButton.
        /// 
        /// <para>Reads the length of red and green lights in ms. Accepts only positive integer inputs, however, small values (e.g. 50ms) are not recommended.</para>
        /// 
        /// <para>Calles ConfigLightLength method of controller.</para>
        /// </summary>
        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            int redLength, greenLength;
            if (int.TryParse(RedLengthEntry.Text, out redLength) && redLength > 0
                && int.TryParse(GreenLengthEntry.Text, out greenLength) && greenLength > 0)
            {
                _controller.ConfigLightLength(redLength, greenLength);

                ShowAlert("Success", "Configuration saved.", "OK");
            }
            else
            {
                ShowAlert("Error", "Please enter valid numbers.", "OK");
            }
        }

        /// <summary>
        /// Handles the click event of the StartButton and starts the traffic light system.
        /// 
        /// <para>Checks PortOpened and LogFileOpened() through the controller.</para>
        /// <para>Calles Start method of controller.</para>
        /// <para>Then it will disable the change of serial port or log file.</para>
        /// 
        /// </summary>
        private async void OnStartButtonClicked(object sender, EventArgs e)
        {
            if (await CheckSetup())
            {
                _controller.Start();

                // Ensure UI updates are done on the UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConfigButton.IsEnabled = true;
                    TickButton.IsEnabled = true;
                    StartButton.IsEnabled = false;
                    DetectSerialPortsButton.IsEnabled = false;
                    OpenLogFileButton.IsEnabled = false;
                    SerialPortPicker.IsEnabled = false;
                    BaudRate.IsEnabled = false;
                    SerialOpenButton.IsEnabled = false;
                });
            }
        }

        /// <summary>
        /// Handles the click event of the OpenLogFileButton and opens a log file.
        /// 
        /// <para>Calls OpenLogFile method of controller.</para>
        /// </summary>
        private async void OnOpenLogFileClicked(object sender, EventArgs e)
        {
            try
            {
                var filePath = await _controller.OpenLogFile();
                LogFilePathLabel.Text = filePath;
                _logFilePath = filePath;
                if (filePath != null)
                {
                    ShowAlert("Success", $"Log file {filePath} opened.", "OK");
                }
                else
                {
                    ShowAlert("Error", $"Log file not opened.", "OK");
                }

            }
            catch (NullReferenceException)
            {
                ShowAlert("Error", $"Log file not opened.", "OK");
            }
        }

        /// <summary>
        /// Handles the click event of the DetectSerialPortsButton and detects serial ports.
        /// <para>Calls GetPortNames method of controller. The detected ports will be made available from the dropdown menu besides.</para>
        /// </summary>
        private async void OnDetectSerialPortsButtonClicked(object sender, EventArgs e)
        {
            string[] ports = await _controller.GetPortNames();

            // Remove disconnected ports
            List<string> toRemove = new List<string>();
            foreach (var port in _serialPorts)
            {
                if (!ports.Contains(port)) toRemove.Add(port);
            }
            foreach (var port in toRemove)
            {
                if (_serialPort == port)
                {
                    _serialPort = null;
                    SerialOpenButton.IsEnabled = false;
                }
                _serialPorts.Remove(port);
            }

            // Add new ports
            for (int i = 0; i < ports.Length; i++)
            {
                var port = ports[i];
                if (!_serialPorts.Contains(port)) _serialPorts.Add(port);
            }

            if (_serialPorts.Count > 0)
            {
                ShowAlert("Success", $"The following serial ports were detected: \n{string.Join("\n", _serialPorts)}", "OK");
            }
            else
            {
                ShowAlert("Info", $"No serial port detected.", "OK");
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the SerialPortPicker and updates the selected serial port.
        /// </summary>
        private void OnSerialPortPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            _serialPort = selectedIndex != -1 ? _serialPorts[selectedIndex] : null;
            if (_serialPort != null)
            {
                SerialOpenButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the click event of the SerialOpenButton and opens a serial port.
        /// <para>Calls OpenPort method of the controller. And checks if a port was already opened through PortOpened property.</para>
        /// </summary>
        private async void OnSerialOpenButtonClicked(object sender, EventArgs e)
        {
            int baudRate;
            if (!int.TryParse(BaudRate.Text, out baudRate))
            {
                ShowAlert("Error", "Please enter a valid baud rate.", "OK");
            }
            else if (_portOpened)
            {
                ShowAlert("Error", "Port already opened.", "OK");
            }
            else if (await _controller.OpenPort(_serialPort, baudRate))
            {
                _portOpened = true;
                ShowAlert("Success", "Port opened successfully.", "OK");
            }
            else
            {
                ShowAlert("Error", $"Failed to open port {_serialPort}.", "OK");
            }
        }

        // Private methods
        private async Task<bool> CheckSetup()
        {
            if (_logFilePath == null)
            {
                // Ensure UI updates are done on the UI thread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    ShowAlert("Error", "Please open a log file first.", "OK");
                });
                return false;
            }

            if (!_portOpened)
            {
                // Ensure UI updates are done on the UI thread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    ShowAlert("Error", "Please open a serial port first.", "OK");
                });
                return false;
            }

            return true;
        }

        private async void ConfigModeEntered(bool success)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (success)
                {
                    UIEnterConfigMode();
                }
                else
                {
                    UIExitConfigMode();
                    ShowAlert("Error", "Cannot enter config mode.", "OK");
                }
            });
        }

        private void UIEnterConfigMode()
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConfigButton.Text = "Exit config mode";
                ConfigButton.IsEnabled = true;
                _configMode = true;
                if (_controller.TaskNumber == TaskNumber.Task3) EnableConfigPanel();
            });
        }

        private void UIExitConfigMode()
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConfigButton.Text = "Config";
                ConfigButton.IsEnabled = true;
                _configMode = false;
                DisableConfigPanel();
            });
        }

        private void DisableConfigPanel()
        {
            RedLengthEntry.IsEnabled = false;
            GreenLengthEntry.IsEnabled = false;
            SaveButton.IsEnabled = false;
        }

        private void EnableConfigPanel()
        {
            RedLengthEntry.IsEnabled = true;
            GreenLengthEntry.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }

        public void HideTickButton()
        {
            TickButton.IsVisible = false;
        }

        // Task methods
        /// <summary>
        /// Sets the traffic light state. This will update the traffic light image.
        /// </summary>
        /// <param name="state">The new state of the traffic light.</param>
        public void SetTrafficLightState(TrafficLightState state)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (state)
                {
                    case TrafficLightState.Red:
                        TrafficLightImage.Source = "traffic_light_red.png";
                        break;
                    case TrafficLightState.Green:
                        TrafficLightImage.Source = "traffic_light_green.png";
                        break;
                    case TrafficLightState.Yellow:
                        TrafficLightImage.Source = "traffic_light_yellow.png";
                        break;
                    case TrafficLightState.None:
                        TrafficLightImage.Source = "traffic_light_blank.png";
                        break;
                }
            });
        }

        /// <summary>
        /// Adds a log entry to the view model. This will update the GUI log entries.
        /// </summary>
        /// <param name="logEntry">The log entry to be added to the view model</param>
        public void AddLogEntry(string logEntry)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.AddLogEntry(logEntry);
            });
        }

        /// <summary>
        /// Sets the log entries in the view model. This will overwrite the entire GUI log entries.
        /// </summary>
        /// <param name="logEntries">The log entries to set.</param>
        public void SetLogEntries(string[] logEntries)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.SetLogEntries(logEntries);
            });
        }

        /// <summary>
        /// Writes the given serial input to the Serial Monitor UI element.
        /// </summary>
        /// <param name="timestamp">The timestamp of the serial input.</param>
        /// <param name="input">The serial input to write.</param>
        public void SerialPrint(DateTime timestamp, string input)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SerialMonitor.Text = $"{timestamp.ToString("o")}\t->\t{input}" + SerialMonitor.Text;
            });
        }

        /// <summary>
        /// Displays an alert window with the given title and message.
        /// </summary>
        /// <param name="title">The title of the alert window.</param>
        /// <param name="message">The message to display in the alert window.</param>
        /// <param name="cancel">The text to display in the cancel button.</param>
        public async void ShowAlert(string title, string message, string cancel)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(title, message, cancel);
            });
        }
    }
}