using FirstApp.Models;
using System.Diagnostics;

namespace FirstApp;

public partial class MainPage
{
    private async void ResetDataClicked(object sender, EventArgs e)
    {
        bool confirmed = await DisplayAlertAsync(
            "Reset all data?",
            "This will permanently delete all saved activity sessions and reset the dashboard. This action cannot be undone.",
            "Reset",
            "Cancel"
        );

        if (!confirmed)
            return;

        try
        {
            _sessions.Clear();
            _visibleSessions.Clear();
            _summaries.Clear();
            _categorySummaries.Clear();

            await _database.DeleteAllSessionsAsync();

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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to reset activity data: {ex.Message}");

            await DisplayAlertAsync(
                "Reset failed",
                "Something went wrong while resetting the activity data.",
                "OK"
            );
        }
    }
}