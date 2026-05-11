using FirstApp.Models;

namespace FirstApp.Services;

public static class ActivityClassifier
{
    public static ActivityCategory Classify(string appName, string title)
    {
        appName = appName?.ToLower() ?? "";
        title = title?.ToLower() ?? "";

        if (appName == "idle")
            return ActivityCategory.Idle;

        if (appName is "devenv" or "code" or "rider64" or "windowsterminal" or "cmd" or "powershell")
            return ActivityCategory.Programming;

        if (appName is "msedge" or "chrome" or "firefox")
        {
            if (title.Contains("youtube") || title.Contains("netflix") || title.Contains("twitch"))
                return ActivityCategory.Entertainment;

            if (title.Contains("github") || title.Contains("stackoverflow") || title.Contains("stack overflow") || title.Contains("learn.microsoft"))
                return ActivityCategory.Programming;

            if (title.Contains("chatgpt") || title.Contains("wikipedia") || title.Contains("coursera") || title.Contains("udemy"))
                return ActivityCategory.Learning;

            if (title.Contains("google docs") || title.Contains("notion") || title.Contains("overleaf"))
                return ActivityCategory.Writing;

            return ActivityCategory.Browsing;
        }

        if (appName is "steam" or "epicgameslauncher" or "riotclientservices")
            return ActivityCategory.Gaming;

        if (appName is "explorer" or "taskmgr")
            return ActivityCategory.System;

        return ActivityCategory.Unknown;
    }

    public static int GetXpPerMinute(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.Programming => 100,
            ActivityCategory.Learning => 80,
            ActivityCategory.Writing => 70,
            ActivityCategory.Browsing => 30,
            ActivityCategory.Entertainment => 20,
            ActivityCategory.Gaming => 50,
            ActivityCategory.System => 10,
            ActivityCategory.Idle => 0,
            _ => 1
        };
    }
}