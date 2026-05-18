using FirstApp.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FirstApp.Services;

public static class ActivityRuleLoader
{
    private static ActivityRuleSet? _cachedRuleSet;

    public static async Task<ActivityRuleSet> LoadRulesAsync()
    {
        if (_cachedRuleSet != null)
            return _cachedRuleSet;

        try
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync(
                "activity-rules.json"
            );

            using var reader = new StreamReader(stream);

            string json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

            _cachedRuleSet = JsonSerializer.Deserialize<ActivityRuleSet>(
                json,
                options
            ) ?? new ActivityRuleSet();

            return _cachedRuleSet;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load activity rules: {ex.Message}");

            _cachedRuleSet = new ActivityRuleSet();
            return _cachedRuleSet;
        }
    }
}