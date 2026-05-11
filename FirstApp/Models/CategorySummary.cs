using FirstApp.Services;
using System.ComponentModel;

namespace FirstApp.Models;

public class CategorySummary : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ActivityCategory Category { get; set; }

    public string CategoryName => Category.ToString();

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
            OnPropertyChanged(nameof(TotalXP));
        }
    }

    public int TotalXP
    {
        get
        {
            int xpPerMinute =
                ActivityClassifier.GetXpPerMinute(Category);

            return (int)(TotalTime.TotalMinutes * xpPerMinute);
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