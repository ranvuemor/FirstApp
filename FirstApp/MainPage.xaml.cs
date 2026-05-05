using FirstApp.Services;

namespace FirstApp
{

    public partial class MainPage : ContentPage
    {
        private async void StartTracking()
        {
            string _lastTitle = null;
            DateTime _startTime = DateTime.Now;
            DateTime _endTime = DateTime.Now;
            TimeSpan duration;
            var _currentApp = "";
            var _currentTitle = "";


            while (true)
            {
                _currentApp = ActiveWindowService.GetCurrentProcessName();
                _currentTitle = ActiveWindowService.GetCurrentWindowTitle();
                if (_lastTitle == null) 
                {
                    _lastTitle = _currentTitle;
                    _startTime = DateTime.Now; 
                }
                
                if (_currentTitle != _lastTitle)
                {
                    _endTime = DateTime.Now;
                    duration = _endTime - _startTime;
                    Console.WriteLine($"App: {_lastTitle}, Duration: {duration}");
                    _lastTitle = _currentTitle;
                    _startTime = DateTime.Now;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveApp.Text = "App: " + _currentApp;
                    ActiveAppLabel.Text = "Title: " + _currentTitle;
                    ActiveAppDuration.Text = "Duration: " + (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");

                });

                await Task.Delay(1000);
            }
        }

        public MainPage()
        {
            var app = ActiveWindowService.GetCurrentProcessName();
            var title = ActiveWindowService.GetCurrentWindowTitle();

            InitializeComponent();
            ActiveApp.Text = "Process: " + app;
            ActiveAppLabel.Text = "Title: " + title;
            StartTracking();

        }

    }
}
