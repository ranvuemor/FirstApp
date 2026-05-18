using FirstApp.Models;
using SQLite;

namespace FirstApp.Services;

public class ActivityDatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        var dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "activity.db3"
        );

        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<ActivitySession>();
    }

    public async Task<List<ActivitySession>> GetSessionsAsync()
    {
        await InitAsync();

        return await _database!
            .Table<ActivitySession>()
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task SaveSessionAsync(ActivitySession session)
    {
        await InitAsync();

        if (session.Id == 0)
            await _database!.InsertAsync(session);
        else
            await _database!.UpdateAsync(session);
    }

    public async Task DeleteAllSessionsAsync()
    {
        await InitAsync();

        await _database!.DeleteAllAsync<ActivitySession>();
    }
}