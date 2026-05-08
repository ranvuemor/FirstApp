namespace FirstApp.Models;

public class AppSummary
{
    public string AppName { get; set; }

    public TimeSpan TotalTime { get; set; }

    public string FormattedTime =>
        FormatDuration(TotalTime);

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 60)
            return $"{(int)duration.TotalSeconds}s";

        if (duration.TotalMinutes < 60)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }
}