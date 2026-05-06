using FirstApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;

namespace FirstApp
{

    public partial class MainPage : ContentPage
    {

        public class ActivitySession : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string AppName { get; set; }
            public string Title { get; set; }
            public DateTime StartTime { get; set; }

            private DateTime _endTime;
            public DateTime EndTime
            {
                get => _endTime;
                set
                {
                    _endTime = value;
                    OnPropertyChanged(nameof(FormattedDuration));
                }
            }

            public string FormattedDuration =>
                FormatDuration(EndTime - StartTime);

            private void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            private string FormatDuration(TimeSpan Duration)
            {
                if (Duration.TotalSeconds < 60)
                    return $"{(int)Math.Round(Duration.TotalSeconds)}s";

                if (Duration.TotalMinutes < 60)
                    return $"{(int)Math.Round(Duration.TotalMinutes)}m {Duration.Seconds}s";

                return $"{(int)Math.Round(Duration.TotalHours)}h {Duration.Minutes}m";
            }
        }

        ObservableCollection<ActivitySession> _sessions = new();
        ActivitySession _currentSession;



        private async void StartTracking()
        {
            string _lastTitle = null;
            string _lastApp = null;
            var _currentApp = "";
            var _currentTitle = "";


            while (true)
            {
                _currentSession.EndTime = DateTime.Now;

                _currentApp = ActiveWindowService.GetCurrentProcessName();
                _currentTitle = ActiveWindowService.GetCurrentWindowTitle();

                if (_lastTitle == null)
                {
                    _lastTitle = _currentTitle;
                    _lastApp = _currentApp;
                }

                if (_currentTitle != _lastTitle)
                {
                    _currentSession.EndTime = DateTime.Now;

                    _currentSession = new ActivitySession
                    {
                        AppName = _currentApp,
                        Title = _currentTitle,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now
                    };

                    _sessions.Add(_currentSession);

                    _lastTitle = _currentTitle;
                    _lastApp = _currentApp;

                    Debug.WriteLine($"Total sessions: {_sessions.Count}");
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveApp.Text = "App: " + _currentApp;
                    ActiveAppLabel.Text = "Title: " + _currentTitle;
                    ActiveAppDuration.Text = "Duration: " + _currentSession.FormattedDuration;
                });
                await Task.Delay(500);
            }
        }

        public MainPage()
        {

            var currentApp = ActiveWindowService.GetCurrentProcessName();
            var currentTitle = ActiveWindowService.GetCurrentWindowTitle();
            _currentSession = new ActivitySession
            {
                AppName = currentApp,
                Title = currentTitle,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };
            _sessions.Add(_currentSession);

            InitializeComponent();

            SessionList.ItemsSource = _sessions;
            StartTracking();

        }

    }
}
