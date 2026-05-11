namespace FirstApp.Services;

public static class LevelService
{
    public static int GetLevel(int totalXp)
    {
        int level = 1;

        while (GetXpRequiredForLevel(level + 1) <= totalXp)
        {
            level++;
        }

        return level;
    }

    public static int GetXpRequiredForLevel(int level)
    {
        if (level <= 1)
            return 0;

        return (level - 1) * (level - 1) * 100;
    }

    public static int GetCurrentLevelXp(int totalXp)
    {
        int level = GetLevel(totalXp);

        return totalXp - GetXpRequiredForLevel(level);
    }

    public static int GetXpNeededForNextLevel(int totalXp)
    {
        int level = GetLevel(totalXp);

        return GetXpRequiredForLevel(level + 1)
            - GetXpRequiredForLevel(level);
    }

    public static double GetLevelProgress(int totalXp)
    {
        int currentLevelXp = GetCurrentLevelXp(totalXp);
        int neededXp = GetXpNeededForNextLevel(totalXp);

        if (neededXp == 0)
            return 0;

        return (double)currentLevelXp / neededXp;
    }
}