using FirstApp.Models;
using Microsoft.Maui.Controls.Shapes;

namespace FirstApp;

public partial class MainPage
{
    private void TryUpdateTimeline(ref DateTime lastUpdate)
    {
        var now = DateTime.Now;

        if ((now - lastUpdate).TotalSeconds <= 0.5)
            return;

        MainThread.BeginInvokeOnMainThread(UpdateTimeline);

        lastUpdate = now;
    }

    private void UpdateTimeline()
    {
        if (TimelineContainer == null)
            return;

        TimelineContainer.Children.Clear();

        foreach (var session in GetVisibleSessions())
        {
            double width = GetTimelineBlockWidth(session);

            var content = CreateTimelineBlockContent(session, width);

            var border = new Border
            {
                WidthRequest = width,
                HeightRequest = 40,
                BackgroundColor = GetColorForCategory(session.Category),
                Padding = 2,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 6
                },
                Content = content
            };

            TimelineContainer.Children.Add(border);
        }
    }

    private double GetTimelineBlockWidth(ActivitySession session)
    {
        double durationSeconds = session.Duration.TotalSeconds;

        return Math.Max(
            30,
            Math.Min(
                500,
                durationSeconds * _timelineZoom
            )
        );
    }

    private VerticalStackLayout CreateTimelineBlockContent(
        ActivitySession session,
        double width)
    {
        var content = new VerticalStackLayout
        {
            Padding = 2,
            Spacing = 0
        };

        if (width < 100)
            return content;

        content.Children.Add(new Label
        {
            Text = $"{session.Category} · {session.DisplayName}",
            FontSize = 10,
            TextColor = Colors.White
        });

        content.Children.Add(new Label
        {
            Text = session.FormattedDuration,
            FontSize = 10,
            TextColor = Colors.White
        });

        return content;
    }

    private void ZoomInClicked(object sender, EventArgs e)
    {
        _timelineZoom += 1;
        UpdateTimeline();
    }

    private void ZoomOutClicked(object sender, EventArgs e)
    {
        _timelineZoom = Math.Max(1, _timelineZoom - 1);
        UpdateTimeline();
    }
}