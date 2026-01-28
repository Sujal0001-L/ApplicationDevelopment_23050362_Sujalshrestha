using SQLite;
using YourJournal.Models;

namespace YourJournal.Services;

/// <summary>
/// Provides data access layer for SQLite database operations.
/// Manages user accounts and journal entries with CRUD operations.
/// </summary>
public class DatabaseService
{
    #region Fields
    
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Initializes a new instance of the DatabaseService.
    /// Database file is stored in the application's data directory.
    /// </summary>
    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "yourjournal.db3");
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Initializes the database connection and creates tables if they don't exist.
    /// Thread-safe initialization pattern ensures single connection instance.
    /// </summary>
    private async Task InitAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<JournalEntry>();
    }
    
    #endregion

    #region User Operations
    
    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <returns>User object if found, null otherwise</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        return await _database!.Table<User>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique ID</param>
    /// <returns>User object if found, null otherwise</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<User>()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Saves or updates a user in the database.
    /// </summary>
    /// <param name="user">The user object to save</param>
    /// <returns>Number of rows affected</returns>
    public async Task<int> SaveUserAsync(User user)
    {
        await InitAsync();
        if (user.Id != 0)
            return await _database!.UpdateAsync(user);
        else
            return await _database!.InsertAsync(user);
    }
    
    #endregion

    #region Journal Entry Operations
    
    /// <summary>
    /// Retrieves all journal entries for a specific user, ordered by date descending.
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>List of journal entries</returns>
    public async Task<List<JournalEntry>> GetEntriesForUserAsync(int userId)
    {
        await InitAsync();
        return await _database!.Table<JournalEntry>()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<JournalEntry?> GetEntryByDateAsync(int userId, DateTime date)
    {
        await InitAsync();
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);
        
        return await _database!.Table<JournalEntry>()
            .Where(e => e.UserId == userId && e.Date >= startOfDay && e.Date < endOfDay)
            .FirstOrDefaultAsync();
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<JournalEntry>()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveEntryAsync(JournalEntry entry)
    {
        await InitAsync();
        entry.UpdatedAt = DateTime.Now;
        entry.WordCount = CountWords(entry.Content);
        
        if (entry.Id != 0)
            return await _database!.UpdateAsync(entry);
        else
        {
            entry.CreatedAt = DateTime.Now;
            return await _database!.InsertAsync(entry);
        }
    }

    public async Task<int> DeleteEntryAsync(JournalEntry entry)
    {
        await InitAsync();
        return await _database!.DeleteAsync(entry);
    }

    public async Task<List<JournalEntry>> SearchEntriesAsync(int userId, string searchText)
    {
        await InitAsync();
        var entries = await _database!.Table<JournalEntry>()
            .Where(e => e.UserId == userId)
            .ToListAsync();
        
        return entries.Where(e => 
            (e.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            e.Content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Date)
            .ToList();
    }

    public async Task<List<JournalEntry>> FilterEntriesAsync(
        int userId, 
        DateTime? startDate = null, 
        DateTime? endDate = null,
        List<string>? moods = null,
        List<string>? tags = null)
    {
        await InitAsync();
        var entries = await _database!.Table<JournalEntry>()
            .Where(e => e.UserId == userId)
            .ToListAsync();

        if (startDate.HasValue)
            entries = entries.Where(e => e.Date >= startDate.Value).ToList();
        
        if (endDate.HasValue)
            entries = entries.Where(e => e.Date <= endDate.Value).ToList();
        
        if (moods != null && moods.Any())
        {
            entries = entries.Where(e => 
                moods.Contains(e.PrimaryMood) ||
                moods.Contains(e.SecondaryMood1 ?? "") ||
                moods.Contains(e.SecondaryMood2 ?? "")).ToList();
        }
        
        if (tags != null && tags.Any())
        {
            entries = entries.Where(e => 
                tags.Any(t => e.Tags?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        return entries.OrderByDescending(e => e.Date).ToList();
    }
    
    #endregion
    
    #region Helper Methods

    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
    
    #endregion
}
