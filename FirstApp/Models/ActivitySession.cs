using FirstApp.Services;
using System.ComponentModel;

namespace FirstApp.Models;

public class ActivitySession : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string AppName { get; set; } = "";

    public string DisplayName => AppNameFormatter.Clean(AppName);

    public string Title { get; set; } = "";

    public DateTime StartTime { get; set; }

    private DateTime _endTime;

    public DateTime EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime == value)
                return;

            _endTime = value;
            OnPropertyChanged(nameof(EndTime));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(FormattedDuration));
        }
    }

    public TimeSpan Duration => EndTime - StartTime;

    public string FormattedDuration => FormatDuration(Duration);

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