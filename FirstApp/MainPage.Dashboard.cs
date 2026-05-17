using FirstApp.Models;
using FirstApp.Services;

namespace FirstApp;

public partial class MainPage
{
    private void RefreshDashboard()
    {
        RebuildVisibleSessions();
        RebuildSummaries();
        RebuildCategorySummaries();

        UpdateLevelUI();
        UpdateUsageChart();
        UpdateTimeline();
        UpdateHourlyHeatmap();
        UpdateDateFilterButtonColors();
        UpdateCollapsedCardPreviews();
        ApplyExpandableCardStates();
        UpdateDashboardSubtitles();
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
        UpdateHourlyHeatmap();
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

    private void ApplyExpandableCardStates()
    {
        DetailsContent.IsVisible = _isDetailsExpanded;
        DetailsChevron.Text = _isDetailsExpanded ? "⌃" : "⌄";
    }

    private void DetailsHeaderTapped(object sender, TappedEventArgs e)
    {
        _isDetailsExpanded = !_isDetailsExpanded;
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

        string topText = topCategory == null
            ? "No data"
            : $"Top: {topCategory.Category}";

        DetailsPreview.Text =
            $"{topText} · {visibleSessions.Count} sessions";
    }

    private void UpdateDashboardSubtitles()
    {
        XpDashboardSubtitle.Text = _selectedDateFilter switch
        {
            DateFilter.Today => "Today's progress",
            DateFilter.Yesterday => "Yesterday's progress",
            DateFilter.ThisWeek => "This week's progress",
            DateFilter.AllTime => "All-time progress",
            _ => "Activity progress"
        };

        DetailsSubtitle.Text = _selectedDateFilter switch
        {
            DateFilter.Today => "Usage chart and session history for today",
            DateFilter.Yesterday => "Usage chart and session history for yesterday",
            DateFilter.ThisWeek => "Usage chart and session history for this week",
            DateFilter.AllTime => "Usage chart and session history across all time",
            _ => "Usage chart and session history"
        };
    }
}