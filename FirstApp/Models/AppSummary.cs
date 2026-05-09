using FirstApp.Services;
using System.ComponentModel;

namespace FirstApp.Models;

public class AppSummary : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string AppName { get; set; } = "";

    public string DisplayName => AppNameFormatter.Clean(AppName);

    private TimeSpan _totalTime;

    public TimeSpan TotalTime
    {
        get => _totalTime;
        set
        {
            if (_totalTime == value)
                return;

            _totalTime = value;
            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(FormattedTime));
        }
    }

    public string FormattedTime => FormatDuration(TotalTime);

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName)
        );
    }

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 60)
            return $"{(int)duration.TotalSeconds}s";

        if (duration.TotalMinutes < 60)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }
}