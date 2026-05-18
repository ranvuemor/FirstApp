using FirstApp.Models;
using FirstApp.Services;

namespace FirstApp;

public partial class MainPage
{
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

    private Color GetColorForCategory(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.Programming => Color.FromArgb("#8B5CF6"),
            ActivityCategory.Learning => Color.FromArgb("#38BDF8"),
            ActivityCategory.ArtificialIntelligence => Color.FromArgb("#10B981"),
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