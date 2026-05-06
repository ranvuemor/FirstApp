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
            public TimeSpan Duration => EndTime - StartTime;

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
            DateTime _lastTimelineUpdate = DateTime.MinValue;

            while (true)
            {
                _currentSession.EndTime = DateTime.Now;
                var (app, title) = ActiveWindowService.GetActiveWindowInfo();

                _currentApp = app;
                _currentTitle = title;

                if (_lastTitle == null)
                {
                    _lastTitle = _currentTitle;
                    _lastApp = _currentApp;
                }

                if (_currentTitle != _lastTitle)
                {
                    _currentSession.EndTime = DateTime.Now;
                    if ((DateTime.Now - _lastTimelineUpdate).TotalSeconds > 2)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UpdateTimeline();
                        });

                        _lastTimelineUpdate = DateTime.Now;
                    }

                    _currentSession = new ActivitySession
                    {
                        AppName = _currentApp,
                        Title = _currentTitle,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now
                    };

                    _sessions.Add(_currentSession);
                    _currentSession.EndTime = DateTime.Now;

                    if ((DateTime.Now - _lastTimelineUpdate).TotalSeconds > 2)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UpdateTimeline();
                        });

                        _lastTimelineUpdate = DateTime.Now;
                    }

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

        string CleanAppName(string app)
        {
            return app.ToLower() switch
            {
                "msedge" => "Edge",
                "chrome" => "Chrome",
                "devenv" => "Visual Studio",
                "explorer" => "Explorer",
                _ => app
            };
        }

        void UpdateTimeline()
        {
            TimelineContainer.Children.Clear();


            foreach (var session in _sessions)
            {
                var durationSeconds = session.Duration.TotalSeconds;

                double width = Math.Max(5, durationSeconds * 5);

                var block = new VerticalStackLayout
                {
                    WidthRequest = width,
                    HeightRequest = 20,
                    BackgroundColor = GetColorForApp(session.AppName),
                    Padding = 2
                };

                block.Children.Add(new Label
                {
                    Text = CleanAppName(session.AppName),
                    FontSize = 5,
                    TextColor = Colors.White
                });

                block.Children.Add(new Label
                {
                    Text = session.FormattedDuration,
                    FontSize = 5,
                    TextColor = Colors.White
                });

                TimelineContainer.Children.Add(block);
            }
        }

        Color GetColorForApp(string appName)
        {
            return appName switch
            {
                "msedge" => Colors.Blue,
                "chrome" => Colors.Red,
                "devenv" => Colors.Purple,
                "explorer" => Colors.Green,
                _ => Colors.Gray
            };
        }

        public MainPage()
        {
            InitializeComponent();
            var (currentApp, currentTitle) = ActiveWindowService.GetActiveWindowInfo();
            _currentSession = new ActivitySession
            {
                AppName = currentApp,
                Title = currentTitle,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };
            _sessions.Add(_currentSession);
            SessionList.ItemsSource = _sessions;
            UpdateTimeline();

            StartTracking();

        }

    }
}
