using FirstApp.Models;
using FirstApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FirstApp;

public partial class MainPage : ContentPage
{
    private readonly ObservableCollection<ActivitySession> _sessions = new();
    private readonly ObservableCollection<ActivitySession> _visibleSessions = new();
    private readonly ObservableCollection<AppSummary> _summaries = new();
    private readonly ObservableCollection<CategorySummary> _categorySummaries = new();

    private readonly ActivityDatabaseService _database = new();
    private readonly CancellationTokenSource _cts = new();

    private ActivitySession _currentSession = null!;

    private double _timelineZoom = 2;
    private DateFilter _selectedDateFilter = DateFilter.Today;
    private bool _isDetailsExpanded;

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
            {
                _sessions.Add(session);
            }

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
}