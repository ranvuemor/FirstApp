namespace FirstApp.Models;

public class ActivityRule
{
    public string Name { get; set; } = "";

    public List<string> Apps { get; set; } = new();

    public List<string> Domains { get; set; } = new();

    public List<string> TitleKeywords { get; set; } = new();

    public ActivityCategory Category { get; set; } = ActivityCategory.Unknown;

    public int Priority { get; set; } = 0;
}