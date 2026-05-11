using FirstApp.Services;
using System.ComponentModel;

namespace FirstApp.Models;

public class ActivitySession : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _appName = "";

    public string AppName
    {
        get => _appName;
        set
        {
            if (_appName == value)
                return;

            _appName = value;
            OnPropertyChanged(nameof(AppName));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(XP));
        }
    }

    public string DisplayName => AppNameFormatter.Clean(AppName);

    private string _title = "";

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value)
                return;

            _title = value;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(XP));
        }
    }

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
            OnPropertyChanged(nameof(XP));
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

    public ActivityCategory Category =>
        ActivityClassifier.Classify(AppName, Title);

    public int XP
    {
        get
        {
            int xpPerMinute =
                ActivityClassifier.GetXpPerMinute(Category);

            return (int)(Duration.TotalMinutes * xpPerMinute);
        }
    }
}