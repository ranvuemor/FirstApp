using FirstApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FirstApp
{

    public partial class MainPage : ContentPage
    {
        
        public class ActivitySession
        {
            public string AppName { get; set; }
            public string Title { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string FormattedDuration =>
                FormatDuration(EndTime - StartTime);
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
        

        private async void StartTracking()
        {
            string _lastTitle = null;
            string _lastApp = null;
            DateTime _startTime = DateTime.Now;
            DateTime _endTime = DateTime.Now;
            TimeSpan duration;
            var _currentApp = "";
            var _currentTitle = "";
            

            while (true)
            {
                _currentApp = ActiveWindowService.GetCurrentProcessName();
                _currentTitle = ActiveWindowService.GetCurrentWindowTitle();
                if (_lastTitle == null) 
                {
                    _lastTitle = _currentTitle;
                    _lastApp = _currentApp;
                    _startTime = DateTime.Now; 
                }
                
                if (_currentTitle != _lastTitle)
                {
                    _sessions.Add(new ActivitySession
                    {
                        AppName = _lastApp,
                        Title = _lastTitle,
                        StartTime = _startTime,
                        EndTime = DateTime.Now

                    });
                    _endTime = DateTime.Now;
                    duration = _endTime - _startTime;
                    Console.WriteLine($"App: {_lastTitle}, Duration: {duration}");
                    _lastTitle = _currentTitle;
                    _lastApp = _currentApp;
                    _startTime = DateTime.Now;
                    Debug.WriteLine($"Total sessions: {_sessions.Count}");

                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveApp.Text = "App: " + _currentApp;
                    ActiveAppLabel.Text = "Title: " + _currentTitle;
                    ActiveAppDuration.Text = "Duration: " + (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");

                    SessionList.ItemsSource = _sessions;

                });

                await Task.Delay(500);
            }
        }

        public MainPage()
        {
            var app = ActiveWindowService.GetCurrentProcessName();
            var title = ActiveWindowService.GetCurrentWindowTitle();

            InitializeComponent();
            //ActiveApp.Text = "Process: " + app;
            //ActiveAppLabel.Text = "Title: " + title;

            SessionList.ItemsSource = _sessions;
            StartTracking();

        }

    }
}
