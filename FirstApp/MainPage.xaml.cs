using FirstApp.Services;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FirstApp.Models;

namespace FirstApp
{

    public partial class MainPage : ContentPage
    {

        ObservableCollection<ActivitySession> _sessions = new();
        ActivitySession _currentSession;


        private CancellationTokenSource _cts = new();

        void TryUpdateTimeline(ref DateTime lastUpdate)
        {
            if ((DateTime.Now - lastUpdate).TotalSeconds > 0.5)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateTimeline();
                });

                lastUpdate = DateTime.Now;
            }
        }

        private async Task StartTracking(CancellationToken token)
        {
            string _lastTitle = null;
            var _currentApp = "";
            var _currentTitle = "";
            DateTime _lastTimelineUpdate = DateTime.MinValue;

            while (!token.IsCancellationRequested)
            {
                _currentSession.EndTime = DateTime.Now;
                TryUpdateTimeline(ref _lastTimelineUpdate);

                var (app, title) = ActiveWindowService.GetActiveWindowInfo();

                _currentApp = app;
                _currentTitle = title;

                if (_lastTitle == null)
                {
                    _lastTitle = _currentTitle;
                }

                if (_currentTitle != _lastTitle)
                {

                    _currentSession = new ActivitySession
                    {
                        AppName = _currentApp,
                        Title = _currentTitle,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now
                    };

                    _sessions.Add(_currentSession);


                    _lastTitle = _currentTitle;

                    Debug.WriteLine($"Total sessions: {_sessions.Count}");
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveApp.Text = "App: " + _currentApp;
                    ActiveAppLabel.Text = "Title: " + _currentTitle;
                    ActiveAppDuration.Text = "Duration: " + _currentSession.FormattedDuration;
                });
                await Task.Delay(500, token);
            }
        }

        string CleanAppName(string app)
        {
            return app.ToLower() switch
            {
                "msedge" => "Edge",
                "chrome" => "Chrome",
                "devenv" => "VS",
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

                double width = Math.Max(80, Math.Min(300, durationSeconds * 2)); ;

                var block = new VerticalStackLayout
                {
                    Padding = 2
                };

                block.Children.Add(new Label
                {
                    Text = CleanAppName(session.AppName),
                    FontSize = 10,
                    TextColor = Colors.White
                });

                block.Children.Add(new Label
                {
                    Text = session.FormattedDuration,
                    FontSize = 10,
                    TextColor = Colors.White
                });

                var border = new Border
                {
                    WidthRequest = width,
                    HeightRequest = 40,
                    BackgroundColor = GetColorForApp(session.AppName),
                    Padding = 2,
                    StrokeThickness = 0,

                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = 6
                    },

                    Content = block
                };

                TimelineContainer.Children.Add(border);
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


            _ = StartTracking(_cts.Token);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _cts.Cancel();
        }

    }
}
