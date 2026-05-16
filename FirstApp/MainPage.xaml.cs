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

    private readonly ActivityDatabaseService _database = new();
    private readonly CancellationTokenSource _cts = new();

    private ActivitySession _currentSession = null!;
    private readonly ObservableCollection<ActivitySession> _visibleSessions = new();

    private IEnumerable<ActivitySession> GetVisibleSessions()
    {
        var today = DateTime.Today;

        return _selectedDateFilter switch
        {
            DateFilter.Today => _sessions
                .Where(s => s.StartTime.Date == today),

            DateFilter.Yesterday => _sessions
                .Where(s => s.StartTime.Date == today.AddDays(-1)),

            DateFilter.ThisWeek => _sessions
                .Where(s => s.StartTime.Date >= GetStartOfWeek(today)),

            DateFilter.AllTime => _sessions,

            _ => _sessions.Where(s => s.StartTime.Date == today)
        };
    }

    private double _timelineZoom = 2;

    private DateFilter _selectedDateFilter = DateFilter.Today;

    private bool _isActivitySummaryExpanded = false;
    private bool _isUsageChartExpanded = false;
    private bool _isSessionHistoryExpanded = false;

    public MainPage()
    {
        InitializeComponent();

        SummaryList.ItemsSource = _summaries;
        CategorySummaryList.ItemsSource = _categorySummaries;
        SessionList.ItemsSource = _visibleSessions;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var savedSessions = await _database.GetSessionsAsync();

            foreach (var session in savedSessions)
                _sessions.Add(session);

            var now = DateTime.Now;
            var currentActivity = GetCurrentActivity();

            _currentSession = new ActivitySession
            {
                AppName = currentActivity.AppName,
                Title = currentActivity.Title,
                Url = currentActivity.Url,
                StartTime = now,
                EndTime = now
            };

            _sessions.Add(_currentSession);

            RefreshDashboard();

            _ = StartTracking(_cts.Token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Initialization failed: {ex.Message}");
        }
    }

    private bool IsSessionVisibleForCurrentFilter(ActivitySession session)
    {
        var today = DateTime.Today;

        return _selectedDateFilter switch
        {
            DateFilter.Today =>
                session.StartTime.Date == today,

            DateFilter.Yesterday =>
                session.StartTime.Date == today.AddDays(-1),

            DateFilter.ThisWeek =>
                session.StartTime.Date >= GetStartOfWeek(today),

            DateFilter.AllTime =>
                true,

            _ =>
                session.StartTime.Date == today
        };
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private void RefreshDashboard()
    {
        RebuildVisibleSessions();
        RebuildSummaries();
        RebuildCategorySummaries();

        UpdateLevelUI();
        UpdateUsageChart();
        UpdateTimeline();
        UpdateDateFilterButtonColors();
        UpdateCollapsedCardPreviews();
        ApplyExpandableCardStates();
    }

    private void ApplyExpandableCardStates()
    {
        ActivitySummaryContent.IsVisible = _isActivitySummaryExpanded;
        UsageChartContent.IsVisible = _isUsageChartExpanded;
        SessionHistoryContent.IsVisible = _isSessionHistoryExpanded;

        ActivitySummaryChevron.Text = _isActivitySummaryExpanded ? "⌃" : "⌄";
        UsageChartChevron.Text = _isUsageChartExpanded ? "⌃" : "⌄";
        SessionHistoryChevron.Text = _isSessionHistoryExpanded ? "⌃" : "⌄";
    }

    private void ActivitySummaryHeaderTapped(object sender, TappedEventArgs e)
    {
        _isActivitySummaryExpanded = !_isActivitySummaryExpanded;
        ApplyExpandableCardStates();
    }

    private void UsageChartHeaderTapped(object sender, TappedEventArgs e)
    {
        _isUsageChartExpanded = !_isUsageChartExpanded;
        ApplyExpandableCardStates();
    }

    private void SessionHistoryHeaderTapped(object sender, TappedEventArgs e)
    {
        _isSessionHistoryExpanded = !_isSessionHistoryExpanded;
        ApplyExpandableCardStates();
    }

    private void UpdateCollapsedCardPreviews()
    {
        var visibleSessions = GetVisibleSessions().ToList();

        TimeSpan totalTime = TimeSpan.FromSeconds(
            visibleSessions.Sum(s => s.Duration.TotalSeconds)
        );

        ActivitySummaryPreview.Text =
            $"{FormatDuration(totalTime)} total";

        var topCategory = visibleSessions
            .GroupBy(s => s.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalSeconds = g.Sum(s => s.Duration.TotalSeconds)
            })
            .OrderByDescending(g => g.TotalSeconds)
            .FirstOrDefault();

        UsageChartPreview.Text = topCategory == null
            ? "No data"
            : $"Top: {topCategory.Category}";

        SessionHistoryPreview.Text =
            $"{visibleSessions.Count} sessions";
    }

    private void RebuildVisibleSessions()
    {
        _visibleSessions.Clear();

        foreach (var session in GetVisibleSessions().OrderByDescending(s => s.StartTime))
        {
            _visibleSessions.Add(session);
        }
    }

    private void RebuildSummaries()
    {
        _summaries.Clear();

        var grouped = GetVisibleSessions()
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
            _summaries.Add(new AppSummary
            {
                AppName = item.AppName,
                TotalTime = item.TotalTime
            });
        }
    }

    private void RebuildCategorySummaries()
    {
        _categorySummaries.Clear();

        var grouped = GetVisibleSessions()
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
            _categorySummaries.Add(new CategorySummary
            {
                Category = item.Category,
                TotalTime = item.TotalTime
            });
        }
    }

    private void TodayFilterClicked(object sender, EventArgs e)
    {
        _selectedDateFilter = DateFilter.Today;
        RefreshDashboard();
    }

    private void YesterdayFilterClicked(object sender, EventArgs e)
    {
        _selectedDateFilter = DateFilter.Yesterday;
        RefreshDashboard();
    }

    private void ThisWeekFilterClicked(object sender, EventArgs e)
    {
        _selectedDateFilter = DateFilter.ThisWeek;
        RefreshDashboard();
    }

    private void AllTimeFilterClicked(object sender, EventArgs e)
    {
        _selectedDateFilter = DateFilter.AllTime;
        RefreshDashboard();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        _cts.Cancel();

        if (_currentSession == null)
            return;

        try
        {
            _currentSession.EndTime = DateTime.Now;
            await _database.SaveSessionAsync(_currentSession);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save session on exit: {ex.Message}");
        }
    }

    private async Task StartTracking(CancellationToken token)
    {
        DateTime lastTimelineUpdate = DateTime.MinValue;
        DateTime lastSummaryUpdate = DateTime.MinValue;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                _currentSession.EndTime = now;

                TryUpdateSummaries(ref lastSummaryUpdate);
                TryUpdateTimeline(ref lastTimelineUpdate);

                var currentActivity = GetCurrentActivity();

                Debug.WriteLine($"URL: {currentActivity.Url}");

                var currentCategory = ActivityClassifier.Classify(
                    currentActivity.AppName,
                    currentActivity.Title,
                    currentActivity.Url
                );

                var previousCategory = _currentSession.Category;

                bool hasActivityChanged =
                    currentActivity.AppName != _currentSession.AppName ||
                    currentCategory != previousCategory;

                if (hasActivityChanged)
                {
                    await SwitchToNewSessionAsync(
                        currentActivity.AppName,
                        currentActivity.Title,
                        currentActivity.Url,
                        now,
                        currentCategory
                    );
                }
                else
                {
                    UpdateCurrentSession(
                        currentActivity.Title,
                        currentActivity.Url,
                        now
                    );
                }

                UpdateActiveActivityUI(currentActivity);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Tracking error: {ex.Message}");
            }

            try
            {
                await Task.Delay(500, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private (string AppName, string Title, string Url) GetCurrentActivity()
    {
        var (appName, title) = ActiveWindowService.GetActiveWindowInfo();

        string url = BrowserUrlService.GetActiveBrowserUrl(appName) ?? "";

        bool isIdle = IdleDetectionService.IsIdle(10);

        if (isIdle)
        {
            appName = "Idle";
            title = "User inactive";
            url = "";
        }

        return (appName, title, url);
    }

    private async Task SwitchToNewSessionAsync(
        string appName,
        string title,
        string url,
        DateTime now,
        ActivityCategory category)
    {
        _currentSession.EndTime = now;

        try
        {
            await _database.SaveSessionAsync(_currentSession);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save session: {ex.Message}");
        }

        var newSession = new ActivitySession
        {
            AppName = appName,
            Title = title,
            Url = url,
            StartTime = now,
            EndTime = now
        };

        _currentSession = newSession;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _sessions.Add(newSession);

            if (IsSessionVisibleForCurrentFilter(newSession))
            {
                _visibleSessions.Insert(0, newSession);
            }
        });

        Debug.WriteLine($"New session: {appName} / {category}");
    }

    private void UpdateCurrentSession(
        string title,
        string url,
        DateTime now)
    {
        _currentSession.Title = title;
        _currentSession.Url = url;
        _currentSession.EndTime = now;
    }

    private void UpdateActiveActivityUI(
        (string AppName, string Title, string Url) activity)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ActiveApp.Text = $"App: {CleanAppName(activity.AppName)}";
            ActiveAppLabel.Text = $"Title: {activity.Title}";
            ActiveAppDuration.Text = $"Duration: {_currentSession.FormattedDuration}";
        });
    }

    private void TryUpdateTimeline(ref DateTime lastUpdate)
    {
        var now = DateTime.Now;

        if ((now - lastUpdate).TotalSeconds <= 0.5)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateTimeline);

        lastUpdate = now;
    }

    private void TryUpdateSummaries(ref DateTime lastUpdate)
    {
        var now = DateTime.Now;

        if ((now - lastUpdate).TotalSeconds <= 1)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateSummaries);

        lastUpdate = now;
    }

    private void UpdateSummaries()
    {
        var grouped = GetVisibleSessions()
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

        UpdateCategorySummaries();
        UpdateLevelUI();
        UpdateUsageChart();
        UpdateCollapsedCardPreviews();
    }

    private void UpdateCategorySummaries()
    {
        var grouped = GetVisibleSessions()
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
        return GetVisibleSessions().Sum(s => s.XP);
    }

    private void UpdateTimeline()
    {
        if (TimelineContainer == null)
            return;

        TimelineContainer.Children.Clear();

        foreach (var session in GetVisibleSessions())
        {
            double width = GetTimelineBlockWidth(session);

            var content = CreateTimelineBlockContent(session, width);

            var border = new Border
            {
                WidthRequest = width,
                HeightRequest = 40,
                BackgroundColor = GetColorForCategory(session.Category),
                Padding = 2,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 6
                },
                Content = content
            };

            TimelineContainer.Children.Add(border);
        }
    }

    private double GetTimelineBlockWidth(ActivitySession session)
    {
        double durationSeconds = session.Duration.TotalSeconds;

        return Math.Max(
            30,
            Math.Min(
                500,
                durationSeconds * _timelineZoom
            )
        );
    }

    private VerticalStackLayout CreateTimelineBlockContent(
        ActivitySession session,
        double width)
    {
        var content = new VerticalStackLayout
        {
            Padding = 2,
            Spacing = 0
        };

        if (width < 100)
            return content;

        content.Children.Add(new Label
        {
            Text = $"{session.Category} · {session.DisplayName}",
            FontSize = 10,
            TextColor = Colors.White
        });

        content.Children.Add(new Label
        {
            Text = session.FormattedDuration,
            FontSize = 10,
            TextColor = Colors.White
        });

        return content;
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

        var grouped = GetVisibleSessions()
            .GroupBy(s => s.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalTime = TimeSpan.FromSeconds(
                    g.Sum(s => s.Duration.TotalSeconds)
                )
            })
            .OrderByDescending(s => s.TotalTime)
            .ToList();

        if (grouped.Count == 0)
            return;

        double maxSeconds = grouped.Max(s => s.TotalTime.TotalSeconds);

        if (maxSeconds <= 0)
            return;

        foreach (var item in grouped)
        {
            var row = CreateUsageChartRow(
                item.Category,
                item.TotalTime,
                maxSeconds
            );

            UsageChartContainer.Children.Add(row);
        }
    }

    private Grid CreateUsageChartRow( 
        ActivityCategory category, 
        TimeSpan totalTime,
        double maxSeconds)
    {
        double seconds = totalTime.TotalSeconds;

        double percentage = maxSeconds <= 0
            ? 0
            : seconds / maxSeconds;

        percentage = Math.Clamp(percentage, 0, 1);

        var row = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = 130 },
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = 70 }
        },
            ColumnSpacing = 10,
            Padding = new Thickness(0, 4),
            HorizontalOptions = LayoutOptions.Fill
        };

        var nameLabel = new Label
        {
            Text = category.ToString(),
            TextColor = Colors.White,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var barContainer = new Grid
        {
            HeightRequest = 16,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            ColumnDefinitions =
        {
            new ColumnDefinition
            {
                Width = new GridLength(Math.Max(percentage, 0.001), GridUnitType.Star)
            },
            new ColumnDefinition
            {
                Width = new GridLength(Math.Max(1 - percentage, 0.001), GridUnitType.Star)
            }
        }
        };

        var bar = new Border
        {
            BackgroundColor = GetColorForCategory(category),
            StrokeThickness = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 8
            }
        };

        barContainer.Add(bar, 0, 0);

        var timeLabel = new Label
        {
            Text = FormatDuration(totalTime),
            TextColor = Colors.LightGray,
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center
        };

        row.Add(nameLabel, 0, 0);
        row.Add(barContainer, 1, 0);
        row.Add(timeLabel, 2, 0);

        return row;
    }

    private void ZoomInClicked(object sender, EventArgs e)
    {
        _timelineZoom += 1;
        UpdateTimeline();
    }

    private void ZoomOutClicked(object sender, EventArgs e)
    {
        _timelineZoom = Math.Max(1, _timelineZoom - 1);
        UpdateTimeline();
    }

    private string CleanAppName(string app)
    {
        return AppNameFormatter.Clean(app);
    }

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 60)
            return $"{(int)duration.TotalSeconds}s";

        if (duration.TotalMinutes < 60)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }

    private void UpdateDateFilterButtonColors()
    {
        Color selectedColor = Color.FromArgb("#2563EB");
        Color normalColor = Color.FromArgb("#1E293B");

        TodayButton.BackgroundColor =
            _selectedDateFilter == DateFilter.Today
                ? selectedColor
                : normalColor;

        YesterdayButton.BackgroundColor =
            _selectedDateFilter == DateFilter.Yesterday
                ? selectedColor
                : normalColor;

        ThisWeekButton.BackgroundColor =
            _selectedDateFilter == DateFilter.ThisWeek
                ? selectedColor
                : normalColor;

        AllTimeButton.BackgroundColor =
            _selectedDateFilter == DateFilter.AllTime
                ? selectedColor
                : normalColor;
    }

    private Color GetColorForCategory(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.Programming => Color.FromArgb("#8B5CF6"),
            ActivityCategory.Learning => Color.FromArgb("#38BDF8"),
            ActivityCategory.Writing => Color.FromArgb("#F59E0B"),
            ActivityCategory.Browsing => Color.FromArgb("#06B6D4"),
            ActivityCategory.Entertainment => Color.FromArgb("#EF4444"),
            ActivityCategory.Gaming => Color.FromArgb("#22C55E"),
            ActivityCategory.System => Color.FromArgb("#64748B"),
            ActivityCategory.Idle => Color.FromArgb("#374151"),
            _ => Color.FromArgb("#6B7280")
        };
    }
}