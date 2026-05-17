using FirstApp.Models;
using FirstApp.Services;
using System.Diagnostics;

namespace FirstApp;

public partial class MainPage
{
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
}