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

    private readonly ObservableCollection<CategorySummary> _categorySummaries = new();

    private readonly CancellationTokenSource _cts = new();

    private ActivitySession _currentSession;

    private double _timelineZoom = 2;

    public MainPage()
    {
        InitializeComponent();

        var (currentApp, currentTitle) = ActiveWindowService.GetActiveWindowInfo();

        SummaryList.ItemsSource = _summaries;
        CategorySummaryList.ItemsSource = _categorySummaries;

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

            var currentCategory =
                ActivityClassifier.Classify(currentApp, currentTitle);

            _currentSession.Title = currentTitle;
            if (currentApp != _currentSession.AppName ||
                currentCategory != _currentSession.Category)
            {
                _currentSession = new ActivitySession
                {
                    AppName = currentApp,
                    Title = currentTitle,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now
                };

                _sessions.Add(_currentSession);

                Debug.WriteLine($"New session: {currentApp} / {currentCategory}");
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
        UpdateCategorySummaries();

        UpdateLevelUI();
    }

    private void TryUpdateSummaries(ref DateTime lastUpdate)
    {
        if ((DateTime.Now - lastUpdate).TotalSeconds <= 1)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateSummaries);

        lastUpdate = DateTime.Now;
    }

    private void UpdateCategorySummaries()
    {
        var grouped = _sessions
            .GroupBy(s => s.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalTime = TimeSpan.FromSeconds(
                    g.Sum(s => s.Duration.TotalSeconds)
                )
            })
            .OrderByDescending(s =>
                ActivityClassifier.GetXpPerMinute(s.Category) *
                s.TotalTime.TotalMinutes
            )
            .ToList();

        foreach (var item in grouped)
        {
            var existing = _categorySummaries
                .FirstOrDefault(s => s.Category == item.Category);

            if (existing == null)
            {
                _categorySummaries.Add(new CategorySummary
                {
                    Category = item.Category,
                    TotalTime = item.TotalTime
                });
            }
            else
            {
                existing.TotalTime = item.TotalTime;
            }
        }
    }

    private int GetTotalXP()
    {
        return _sessions.Sum(s => s.XP);
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

    private void UpdateLevelUI()
    {
        int totalXp = GetTotalXP();

        int level = LevelService.GetLevel(totalXp);
        int currentLevelXp = LevelService.GetCurrentLevelXp(totalXp);
        int xpNeeded = LevelService.GetXpNeededForNextLevel(totalXp);
        double progress = LevelService.GetLevelProgress(totalXp);

        TotalXPLabel.Text = $"Total XP: {totalXp}";
        LevelLabel.Text = $"Level {level}";
        LevelProgressLabel.Text = $"{currentLevelXp} / {xpNeeded} XP";

        LevelProgressBar.Progress = progress;
    }

    private void UpdateUsageChart()
    {
        if (UsageChartContainer == null)
            return;

        UsageChartContainer.Children.Clear();

        if (_summaries.Count == 0)
            return;

        double maxSeconds = _summaries
            .Max(s => s.TotalTime.TotalSeconds);

        if (maxSeconds <= 0)
            return;

        double availableWidth = UsageChartContainer.Width;

        if (double.IsNaN(availableWidth) || availableWidth <= 0)
            availableWidth = 400;

        double maxBarWidth = Math.Max(60, availableWidth - 180);

        foreach (var summary in _summaries)
        {
            double seconds = summary.TotalTime.TotalSeconds;

            double barWidth =
                Math.Max(20, (seconds / maxSeconds) * maxBarWidth);

            var row = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = 90 },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 70 }
            },
                ColumnSpacing = 10,
                Padding = new Thickness(0, 4),
                HorizontalOptions = LayoutOptions.Fill
            };

            var nameLabel = new Label
            {
                Text = AppNameFormatter.Clean(summary.AppName),
                TextColor = Colors.White,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            var barContainer = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
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

            barContainer.Children.Add(bar);

            var timeLabel = new Label
            {
                Text = summary.FormattedTime,
                TextColor = Colors.LightGray,
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.End,
                VerticalOptions = LayoutOptions.Center
            };

            row.Add(nameLabel, 0, 0);
            row.Add(barContainer, 1, 0);
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

    private Color GetColorForApp(string appName)
    {
        return appName.ToLower() switch
        {
            "msedge" => Color.FromArgb("#CC2563EB"),
            "chrome" => Color.FromArgb("#CCDC2626"),
            "devenv" => Color.FromArgb("#CC7C3AED"),
            "explorer" => Color.FromArgb("#CC16A34A"),
            "idle" => Color.FromArgb("#CC4B5563"),
            _ => Color.FromArgb("#CC6B7280")
        };
    }
}