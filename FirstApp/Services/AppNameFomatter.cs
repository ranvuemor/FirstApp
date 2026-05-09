namespace FirstApp.Services;

public static class AppNameFormatter
{
    public static string Clean(string app)
    {
        if (string.IsNullOrWhiteSpace(app))
            return "Unknown";

        return app.ToLower() switch
        {
            "msedge" => "Edge",
            "chrome" => "Chrome",
            "devenv" => "Visual Studio",
            "explorer" => "Explorer",
            "idle" => "Idle",
            _ => app
        };
    }
}