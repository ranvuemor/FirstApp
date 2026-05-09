using FirstApp.Models;
using FirstApp.Services;

using Microsoft.Maui.Controls.Shapes;

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FirstApp;

public partial class MainPage : ContentPage
{
    private readonly ObservableCollection<ActivitySession> _sessions = new();

    private readonly ObservableCollection<AppSummary> _summaries = new();

    private readonly CancellationTokenSource _cts = new();

    private ActivitySession _currentSession;

    private double _timelineZoom = 2;

    public MainPage()
    {
        InitializeComponent();

        var (currentApp, currentTitle) = ActiveWindowService.GetActiveWindowInfo();

        SummaryList.ItemsSource = _summaries;

        _currentSession = new ActivitySession
        {
            AppName = currentApp,
            Title = currentTitle,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now
        };

        _sessions.Add(_currentSession);

        SessionList.ItemsSource = _sessions;

        UpdateSummaries();
        UpdateTimeline();

        _ = StartTracking(_cts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _cts.Cancel();
    }

    private async Task StartTracking(CancellationToken token)
    {
        DateTime lastTimelineUpdate = DateTime.MinValue;
        DateTime lastSummaryUpdate = DateTime.MinValue;

        while (!token.IsCancellationRequested)
        {
            _currentSession.EndTime = DateTime.Now;

            TryUpdateSummaries(ref lastSummaryUpdate);

            TryUpdateTimeline(ref lastTimelineUpdate);

            var (currentApp, currentTitle) = ActiveWindowService.GetActiveWindowInfo();

            bool isIdle = IdleDetectionService.IsIdle(10);

            if (isIdle)
            {
                currentApp = "Idle";
                currentTitle = "User inactive";
            }

            if (currentApp != _currentSession.AppName)
            {
                _currentSession =
                    new ActivitySession
                    {
                        AppName = currentApp,
                        Title = currentTitle,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now
                    };

                _sessions.Add(_currentSession);

                Debug.WriteLine(
                    $"New session: {currentApp}"
                );
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ActiveApp.Text = $"App: {CleanAppName(currentApp)}";

                ActiveAppLabel.Text = $"Title: {currentTitle}";

                ActiveAppDuration.Text = $"Duration: {_currentSession.FormattedDuration}";
            });

            await Task.Delay(500, token);
        }
    }

    private void TryUpdateTimeline(
        ref DateTime lastUpdate)
    {
        if ((DateTime.Now - lastUpdate)
            .TotalSeconds <= 0.5)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateTimeline);

        lastUpdate = DateTime.Now;
    }

    private void UpdateSummaries()
    {
        var grouped = _sessions
            .GroupBy(s => s.AppName)
            .Select(g => new
            {
                AppName = g.Key,
                TotalTime = TimeSpan.FromSeconds(
                    g.Sum(s => s.Duration.TotalSeconds)
                )
            })
            .OrderByDescending(s => s.TotalTime)
            .ToList();

        foreach (var item in grouped)
        {
            var existing = _summaries
                .FirstOrDefault(s => s.AppName == item.AppName);

            if (existing == null)
            {
                _summaries.Add(new AppSummary
                {
                    AppName = item.AppName,
                    TotalTime = item.TotalTime
                });
            }
            else
            {
                existing.TotalTime = item.TotalTime;
            }
        }
        UpdateUsageChart();
    }

    private void TryUpdateSummaries(ref DateTime lastUpdate)
    {
        if ((DateTime.Now - lastUpdate).TotalSeconds <= 1)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateSummaries);

        lastUpdate = DateTime.Now;
    }

    private void UpdateTimeline()
    {
        if (TimelineContainer == null)
            return;

        TimelineContainer.Children.Clear();

        foreach (var session in _sessions)
        {
            double durationSeconds = session.Duration.TotalSeconds;

            double width =
                Math.Max(
                    30,
                    Math.Min(
                        500,
                        durationSeconds * _timelineZoom
                    )
                );

            var content = new VerticalStackLayout
            {
                Padding = 2,
                Spacing = 0
            };

            if (width >= 100)
            {
                content.Children.Add(new Label
                {
                    Text =
                        CleanAppName(
                            session.AppName),

                    FontSize = 10,
                    TextColor = Colors.White
                });

                content.Children.Add(new Label
                {
                    Text =
                        session.FormattedDuration,

                    FontSize = 10,
                    TextColor = Colors.White
                });
            }

            var border = new Border
            {
                WidthRequest = width,
                HeightRequest = 40,

                BackgroundColor =
                    GetColorForApp(
                        session.AppName),

                Padding = 2,
                StrokeThickness = 0,

                StrokeShape =
                    new RoundRectangle
                    {
                        CornerRadius = 6
                    },

                Content = content
            };

            TimelineContainer.Children.Add(border);
        }
    }

    private void UpdateUsageChart()
    {
        if (UsageChartContainer == null)
            return;

        UsageChartContainer.Children.Clear();

        if (_summaries.Count == 0)
            return;

        double maxSeconds =
            _summaries.Max(s => s.TotalTime.TotalSeconds);

        if (maxSeconds <= 0)
            return;

        foreach (var summary in _summaries)
        {
            double seconds =
                summary.TotalTime.TotalSeconds;

            double barWidth =
                Math.Max(20, (seconds / maxSeconds) * 300);

            var row = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = 90 },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 70 }
            },
                ColumnSpacing = 10,
                Padding = new Thickness(0, 4)
            };

            var nameLabel = new Label
            {
                Text = AppNameFormatter.Clean(summary.AppName),
                TextColor = Colors.White,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center
            };

            var bar = new Border
            {
                WidthRequest = barWidth,
                HeightRequest = 16,
                BackgroundColor = GetColorForApp(summary.AppName),
                StrokeThickness = 0,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,

                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 8
                }
            };

            var timeLabel = new Label
            {
                Text = summary.FormattedTime,
                TextColor = Colors.LightGray,
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.End,
                VerticalOptions = LayoutOptions.Center
            };

            row.Add(nameLabel, 0, 0);
            row.Add(bar, 1, 0);
            row.Add(timeLabel, 2, 0);

            UsageChartContainer.Children.Add(row);
        }
    }

    private void ZoomInClicked(
        object sender,
        EventArgs e)
    {
        _timelineZoom += 1;

        UpdateTimeline();
    }

    private void ZoomOutClicked(
        object sender,
        EventArgs e)
    {
        _timelineZoom =
            Math.Max(1, _timelineZoom - 1);

        UpdateTimeline();
    }

    private string CleanAppName(string app)
    {
        return AppNameFormatter.Clean(app);
    }

    private Color GetColorForApp(
        string appName)
    {
        return appName.ToLower() switch
        {
            "msedge" => Colors.Blue,
            "chrome" => Colors.Red,
            "devenv" => Colors.Purple,
            "explorer" => Colors.Green,
            "idle" => Colors.DarkGray,
            _ => Colors.Gray
        };
    }
}