using FirstApp.Models;

namespace FirstApp.Services;

public static class ActivityClassifier
{
    public static ActivityCategory Classify(string appName)
    {
        if (string.IsNullOrWhiteSpace(appName))
            return ActivityCategory.Unknown;

        return appName.ToLower() switch
        {
            "devenv" => ActivityCategory.Programming,
            "code" => ActivityCategory.Programming,
            "rider64" => ActivityCategory.Programming,
            "terminal" => ActivityCategory.Programming,
            "windowsterminal" => ActivityCategory.Programming,
            "cmd" => ActivityCategory.Programming,
            "powershell" => ActivityCategory.Programming,

            "msedge" => ActivityCategory.Browsing,
            "chrome" => ActivityCategory.Browsing,
            "firefox" => ActivityCategory.Browsing,

            "steam" => ActivityCategory.Gaming,
            "epicgameslauncher" => ActivityCategory.Gaming,
            "riotclientservices" => ActivityCategory.Gaming,

            "explorer" => ActivityCategory.System,
            "taskmgr" => ActivityCategory.System,

            "idle" => ActivityCategory.Idle,

            _ => ActivityCategory.Unknown
        };
    }

    public static int GetXpPerMinute(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.Programming => 100,
            ActivityCategory.Browsing => 30,
            ActivityCategory.Gaming => 50,
            ActivityCategory.System => 10,
            ActivityCategory.Idle => 0,
            _ => 1
        };
    }
}