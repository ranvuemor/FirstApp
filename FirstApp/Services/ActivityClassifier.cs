using FirstApp.Models;
using System.Diagnostics;

namespace FirstApp.Services;

public static class ActivityClassifier
{
    private static ActivityRuleSet _rules = new();
    private static bool _isInitialized;

    public static async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        _rules = await ActivityRuleLoader.LoadRulesAsync();
        _isInitialized = true;

        Debug.WriteLine($"Loaded {_rules.Rules.Count} activity rules.");
    }

    public static ActivityCategory Classify(
        string appName,
        string title,
        string url)
    {
        if (!_isInitialized)
        {
            Debug.WriteLine("ActivityClassifier was used before initialization.");
        }

        appName = Normalize(appName);
        title = Normalize(title);
        url = Normalize(url);

        var matchingRule = _rules.Rules
            .Where(rule => RuleMatches(rule, appName, title, url))
            .OrderByDescending(rule => rule.Priority)
            .FirstOrDefault();

        if (matchingRule != null)
            return matchingRule.Category;

        if (IsBrowser(appName))
            return ActivityCategory.Browsing;

        return ActivityCategory.Unknown;
    }

    public static int GetXpPerMinute(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.Programming => 100,
            ActivityCategory.Learning => 80,
            ActivityCategory.ArtificialIntelligence => 80,
            ActivityCategory.Writing => 70,
            ActivityCategory.Gaming => 50,
            ActivityCategory.Browsing => 30,
            ActivityCategory.Entertainment => 20,
            ActivityCategory.System => 10,
            ActivityCategory.Unknown => 10,
            ActivityCategory.Idle => 0,
            _ => 1
        };
    }

    private static bool RuleMatches(
        ActivityRule rule,
        string appName,
        string title,
        string url)
    {
        bool hasAppRules = rule.Apps.Count > 0;
        bool hasDomainRules = rule.Domains.Count > 0;
        bool hasTitleRules = rule.TitleKeywords.Count > 0;

        bool appMatches =
            !hasAppRules ||
            rule.Apps.Any(app => appName.Contains(Normalize(app)));

        bool domainMatches =
            !hasDomainRules ||
            rule.Domains.Any(domain => UrlContainsDomain(url, domain));

        bool titleMatches =
            !hasTitleRules ||
            rule.TitleKeywords.Any(keyword =>
                title.Contains(Normalize(keyword)) ||
                url.Contains(Normalize(keyword))
            );

        return appMatches && domainMatches && titleMatches;
    }

    private static bool UrlContainsDomain(string url, string domain)
    {
        url = Normalize(url);
        domain = Normalize(domain);

        if (string.IsNullOrWhiteSpace(url))
            return false;

        return url.Contains(domain);
    }

    private static bool IsBrowser(string appName)
    {
        return appName.Contains("msedge") ||
               appName.Contains("chrome") ||
               appName.Contains("firefox") ||
               appName.Contains("brave") ||
               appName.Contains("opera") ||
               appName.Contains("vivaldi");
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}