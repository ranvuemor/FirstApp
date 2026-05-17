using FirstApp.Models;
using Microsoft.Maui.Controls.Shapes;

namespace FirstApp;

public partial class MainPage
{
    private void UpdateHourlyHeatmap()
    {
        if (HourlyHeatmapContainer == null)
            return;

        HourlyHeatmapContainer.Children.Clear();

        var visibleSessions = GetVisibleSessions().ToList();

        var hourlyCategorySeconds = Enumerable
            .Range(0, 24)
            .Select(_ => new Dictionary<ActivityCategory, double>())
            .ToArray();

        foreach (var session in visibleSessions)
        {
            AddSessionToHourlyCategoryBuckets(session, hourlyCategorySeconds);
        }

        int dayCount = GetSelectedDateRangeDayCount();

        if (dayCount <= 0)
            dayCount = 1;

        double[] hourlyAverageTotals = hourlyCategorySeconds
            .Select(hour => hour.Values.Sum() / dayCount)
            .ToArray();

        double maxAverageSeconds = hourlyAverageTotals.Max();
        double totalAverageSecondsPerDay = hourlyAverageTotals.Sum();

        HeatmapPreview.Text =
            $"{FormatDuration(TimeSpan.FromSeconds(totalAverageSecondsPerDay))}/day avg";

        for (int hour = 0; hour < 24; hour++)
        {
            var categoryData = hourlyCategorySeconds[hour];

            double averageTotalSeconds = hourlyAverageTotals[hour];

            ActivityCategory dominantCategory =
                GetDominantCategory(categoryData);

            double intensity = maxAverageSeconds <= 0
                ? 0
                : averageTotalSeconds / maxAverageSeconds;

            var block = CreateHourlyHeatmapBlock(
                hour,
                averageTotalSeconds,
                intensity,
                dominantCategory
            );

            HourlyHeatmapContainer.Children.Add(block);
        }
    }

    private void AddSessionToHourlyCategoryBuckets(
        ActivitySession session,
        Dictionary<ActivityCategory, double>[] hourlyCategorySeconds)
    {
        DateTime start = session.StartTime;
        DateTime end = session.EndTime;

        if (end <= start)
            return;

        DateTime cursor = start;
        ActivityCategory category = session.Category;

        while (cursor < end)
        {
            DateTime nextHour = new DateTime(
                cursor.Year,
                cursor.Month,
                cursor.Day,
                cursor.Hour,
                0,
                0
            ).AddHours(1);

            DateTime segmentEnd = end < nextHour
                ? end
                : nextHour;

            double seconds = (segmentEnd - cursor).TotalSeconds;

            int hour = cursor.Hour;

            if (!hourlyCategorySeconds[hour].ContainsKey(category))
            {
                hourlyCategorySeconds[hour][category] = 0;
            }

            hourlyCategorySeconds[hour][category] += seconds;

            cursor = segmentEnd;
        }
    }

    private ActivityCategory GetDominantCategory(
        Dictionary<ActivityCategory, double> categoryData)
    {
        if (categoryData.Count == 0)
            return ActivityCategory.Unknown;

        return categoryData
            .OrderByDescending(c => c.Value)
            .First()
            .Key;
    }

    private int GetSelectedDateRangeDayCount()
    {
        return _selectedDateFilter switch
        {
            DateFilter.Today => 1,
            DateFilter.Yesterday => 1,
            DateFilter.ThisWeek => 7,
            DateFilter.AllTime => GetAllTimeDayCount(),
            _ => 1
        };
    }

    private int GetAllTimeDayCount()
    {
        if (_sessions.Count == 0)
            return 1;

        DateTime firstDay = _sessions
            .Min(s => s.StartTime.Date);

        DateTime lastDay = DateTime.Today;

        int days = (lastDay - firstDay).Days + 1;

        return Math.Max(1, days);
    }

    private View CreateHourlyHeatmapBlock(
        int hour,
        double seconds,
        double intensity,
        ActivityCategory dominantCategory)
    {
        Color blockColor = GetHeatmapCategoryColor(
            dominantCategory,
            intensity
        );

        string timeText = $"{hour:00}";

        string durationText = seconds <= 0
            ? "0s"
            : FormatDuration(TimeSpan.FromSeconds(seconds));

        var block = new VerticalStackLayout
        {
            Spacing = 6,
            WidthRequest = 48,
            HorizontalOptions = LayoutOptions.Center
        };

        block.Children.Add(new Label
        {
            Text = timeText,
            FontSize = 11,
            TextColor = Color.FromArgb("#94A3B8"),
            HorizontalTextAlignment = TextAlignment.Center
        });

        block.Children.Add(new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            BackgroundColor = blockColor,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 10
            }
        });

        block.Children.Add(new Label
        {
            Text = durationText,
            FontSize = 10,
            TextColor = Color.FromArgb("#CBD5E1"),
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        return block;
    }

    private Color GetHeatmapCategoryColor(
        ActivityCategory category,
        double intensity)
    {
        if (intensity <= 0)
            return Color.FromArgb("#1E293B");

        Color baseColor = GetColorForCategory(category);

        double alpha = 0.25 + (0.75 * intensity);

        return baseColor.WithAlpha((float)alpha);
    }
}