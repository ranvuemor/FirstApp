using FirstApp.Models;
using FirstApp.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FirstApp;

public partial class MainPage
{
    private void TryUpdateSummaries(ref DateTime lastUpdate)
    {
        var now = DateTime.Now;

        if ((now - lastUpdate).TotalSeconds <= 1)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateSummaries);

        lastUpdate = now;
    }

    private int GetTotalXP()
    {
        return GetVisibleSessions().Sum(s => s.XP);
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
                    Width = new GridLength(
                        Math.Max(percentage, 0.001),
                        GridUnitType.Star
                    )
                },
                new ColumnDefinition
                {
                    Width = new GridLength(
                        Math.Max(1 - percentage, 0.001),
                        GridUnitType.Star
                    )
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
}