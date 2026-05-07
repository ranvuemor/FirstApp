using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FirstApp.Models
{
        public class ActivitySession : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public TimeSpan Duration => EndTime - StartTime;

            public string AppName { get; set; }
            public string Title { get; set; }
            public DateTime StartTime { get; set; }

            private DateTime _endTime;
            public DateTime EndTime
            {
                get => _endTime;
                set
                {
                    _endTime = value;
                    OnPropertyChanged(nameof(FormattedDuration));
                }
            }

            public string FormattedDuration =>
                FormatDuration(EndTime - StartTime);

            private void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            private string FormatDuration(TimeSpan Duration)
            {
                if (Duration.TotalSeconds < 60)
                    return $"{(int)Math.Round(Duration.TotalSeconds)}s";

                if (Duration.TotalMinutes < 60)
                    return $"{(int)Math.Round(Duration.TotalMinutes)}m {Duration.Seconds}s";

                return $"{(int)Math.Round(Duration.TotalHours)}h {Duration.Minutes}m";
            }


        }
    
}
