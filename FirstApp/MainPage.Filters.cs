using FirstApp.Models;

namespace FirstApp;

public partial class MainPage
{
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
}