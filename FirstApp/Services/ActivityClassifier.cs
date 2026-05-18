using FirstApp.Models;

namespace FirstApp.Services;

public static class ActivityClassifier
{
    public static ActivityCategory Classify(string appName, string title, string url)
    {
        appName = appName?.ToLower() ?? "";
        title = title?.ToLower() ?? "";
        url = url?.ToLower() ?? "";

        if (appName == "idle")
            return ActivityCategory.Idle;

        if (appName is "devenv" or "code" or "rider64" or "windowsterminal" or "cmd" or "powershell")
            return ActivityCategory.Programming;

        if (IsSupportedBrowser(appName))
        {
            if (url.Contains("youtube.com") ||
                url.Contains("netflix.com") ||
                url.Contains("twitch.tv") ||
                title.Contains("primevideo.com") ||
                title.Contains("hotstar.com"))
                return ActivityCategory.Entertainment;

            if (url.Contains("github.com") ||
                url.Contains("stackoverflow.com") ||
                url.Contains("learn.microsoft.com"))
                return ActivityCategory.Programming;

            if (url.Contains("chat.openai.com") ||
                url.Contains("chatgpt.com") ||
                url.Contains("openai.com") ||
                url.Contains("claude.ai") ||
                url.Contains("perplexity.ai") ||
                url.Contains("gemini.google.com"))
                return ActivityCategory.ArtificialIntelligence;

            if (url.Contains("docs.google.com") ||
                url.Contains("notion.so") ||
                url.Contains("overleaf.com"))
                return ActivityCategory.Writing;

            // fallback to title if URL was not detected
            if (title.Contains("youtube") ||
                title.Contains("netflix") ||
                title.Contains("twitch") ||
                title.Contains("prime") ||
                title.Contains("hotstar"))
                return ActivityCategory.Entertainment;

            if (title.Contains("github") ||
                title.Contains("stackoverflow") ||
                title.Contains("stack overflow") ||
                title.Contains("learn.microsoft"))
                return ActivityCategory.Programming;

            if (title.Contains("chatgpt") ||
                title.Contains("openai") ||
                title.Contains("claude") ||
                title.Contains("perplexity") ||
                title.Contains("gemini"))
                return ActivityCategory.ArtificialIntelligence;

            return ActivityCategory.Browsing;
        }

        if (appName is "steam" or "epicgameslauncher" or "riotclientservices")
            return ActivityCategory.Gaming;

        if (appName is "explorer" or "taskmgr")
            return ActivityCategory.System;

        return ActivityCategory.Unknown;
    }

    private static bool IsSupportedBrowser(string appName)
    {
        appName = appName.ToLower();

        return appName is
            "msedge" or
            "chrome" or
            "firefox" or
            "brave" or
            "opera" or
            "vivaldi";
    }

    public static int GetXpPerMinute(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.Programming => 100,
            ActivityCategory.Learning => 80,
            ActivityCategory.ArtificialIntelligence => 50,
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