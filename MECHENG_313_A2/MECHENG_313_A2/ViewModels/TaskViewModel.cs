using MECHENG_313_A2.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;

namespace MECHENG_313_A2.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _logEntries;

        public ObservableCollection<string> LogEntries
        {
            get
            {
                return _logEntries;
            }
            set
            {
                if (_logEntries != value)
                {
                    _logEntries = value;
                    OnPropertyChanged(nameof(LogEntries));
                }
            }
        }

        public TaskViewModel()
        {
            LogEntries = new ObservableCollection<string>();
        }

        public void AddLogEntry(string logEntry)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LogEntries.Insert(0, logEntry);
            });
        }

        public void SetLogEntries(string[] logEntries)
        {
            // Ensure UI updates are done on the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LogEntries = new ObservableCollection<string>(logEntries);
            });
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
