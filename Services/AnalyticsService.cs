using YourJournal.Models;

namespace YourJournal.Services;

public class AnalyticsService
{
    private readonly DatabaseService _database;
    private readonly AuthService _authService;

    public AnalyticsService(DatabaseService database, AuthService authService)
    {
        _database = database;
        _authService = authService;
    }

    public async Task<Dictionary<string, int>> GetMoodDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_authService.CurrentUser == null)
            return new Dictionary<string, int>();

        var entries = await GetFilteredEntriesAsync(startDate, endDate);
        
        var distribution = new Dictionary<string, int>
        {
            ["Positive"] = 0,
            ["Neutral"] = 0,
            ["Negative"] = 0
        };

        foreach (var entry in entries)
        {
            var category = GetMoodCategory(entry.PrimaryMood);
            if (!string.IsNullOrEmpty(category))
                distribution[category]++;
        }

        return distribution;
    }

    public async Task<Dictionary<string, int>> GetMoodFrequencyAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_authService.CurrentUser == null)
            return new Dictionary<string, int>();

        var entries = await GetFilteredEntriesAsync(startDate, endDate);
        var frequency = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            CountMood(frequency, entry.PrimaryMood);
            CountMood(frequency, entry.SecondaryMood1);
            CountMood(frequency, entry.SecondaryMood2);
        }

        return frequency.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<string?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var frequency = await GetMoodFrequencyAsync(startDate, endDate);
        return frequency.FirstOrDefault().Key;
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        if (_authService.CurrentUser == null)
            return 0;

        var entries = await _database.GetEntriesForUserAsync(_authService.CurrentUser.Id);
        if (!entries.Any())
            return 0;

        var orderedDates = entries.Select(e => e.Date.Date).Distinct().OrderByDescending(d => d).ToList();
        
        var streak = 0;
        var currentDate = DateTime.Today;

        foreach (var date in orderedDates)
        {
            if (date == currentDate)
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }
            else if (date == currentDate.AddDays(-1))
            {
                streak++;
                currentDate = date.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    public async Task<int> GetLongestStreakAsync()
    {
        if (_authService.CurrentUser == null)
            return 0;

        var entries = await _database.GetEntriesForUserAsync(_authService.CurrentUser.Id);
        if (!entries.Any())
            return 0;

        var orderedDates = entries.Select(e => e.Date.Date).Distinct().OrderBy(d => d).ToList();
        
        int maxStreak = 0;
        int currentStreak = 1;

        for (int i = 1; i < orderedDates.Count; i++)
        {
            if (orderedDates[i] == orderedDates[i - 1].AddDays(1))
            {
                currentStreak++;
            }
            else
            {
                maxStreak = Math.Max(maxStreak, currentStreak);
                currentStreak = 1;
            }
        }

        return Math.Max(maxStreak, currentStreak);
    }

    public async Task<int> GetMissedDaysAsync(int daysRange = 30)
    {
        if (_authService.CurrentUser == null)
            return 0;

        var startDate = DateTime.Today.AddDays(-daysRange);
        var entries = await _database.GetEntriesForUserAsync(_authService.CurrentUser.Id);
        var entryDates = entries.Where(e => e.Date >= startDate).Select(e => e.Date.Date).Distinct().ToHashSet();

        int missedDays = 0;
        for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
        {
            if (!entryDates.Contains(date))
                missedDays++;
        }

        return missedDays;
    }

    public async Task<Dictionary<string, int>> GetTagFrequencyAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_authService.CurrentUser == null)
            return new Dictionary<string, int>();

        var entries = await GetFilteredEntriesAsync(startDate, endDate);
        var frequency = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Tags))
                continue;

            var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entry.Tags);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (!frequency.ContainsKey(tag))
                        frequency[tag] = 0;
                    frequency[tag]++;
                }
            }
        }

        return frequency.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<Dictionary<string, int>> GetCategoryBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_authService.CurrentUser == null)
            return new Dictionary<string, int>();

        var entries = await GetFilteredEntriesAsync(startDate, endDate);
        var breakdown = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Category))
                continue;

            if (!breakdown.ContainsKey(entry.Category))
                breakdown[entry.Category] = 0;
            breakdown[entry.Category]++;
        }

        return breakdown.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<Dictionary<string, double>> GetAverageWordCountTrendAsync(int months = 6)
    {
        if (_authService.CurrentUser == null)
            return new Dictionary<string, double>();

        var entries = await _database.GetEntriesForUserAsync(_authService.CurrentUser.Id);
        var startDate = DateTime.Today.AddMonths(-months);
        
        var trend = new Dictionary<string, double>();

        for (int i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            
            var monthEntries = entries.Where(e => e.Date >= monthStart && e.Date < monthEnd).ToList();
            var avgWordCount = monthEntries.Any() ? monthEntries.Average(e => e.WordCount) : 0;
            
            trend[monthStart.ToString("MMM yyyy")] = Math.Round(avgWordCount, 2);
        }

        return trend;
    }

    private async Task<List<JournalEntry>> GetFilteredEntriesAsync(DateTime? startDate, DateTime? endDate)
    {
        if (_authService.CurrentUser == null)
            return new List<JournalEntry>();

        var entries = await _database.GetEntriesForUserAsync(_authService.CurrentUser.Id);

        if (startDate.HasValue)
            entries = entries.Where(e => e.Date >= startDate.Value).ToList();
        
        if (endDate.HasValue)
            entries = entries.Where(e => e.Date <= endDate.Value).ToList();

        return entries;
    }

    private string? GetMoodCategory(string mood)
    {
        if (MoodData.PositiveMoods.Contains(mood)) return "Positive";
        if (MoodData.NeutralMoods.Contains(mood)) return "Neutral";
        if (MoodData.NegativeMoods.Contains(mood)) return "Negative";
        return null;
    }

    private void CountMood(Dictionary<string, int> frequency, string? mood)
    {
        if (string.IsNullOrEmpty(mood))
            return;

        if (!frequency.ContainsKey(mood))
            frequency[mood] = 0;
        frequency[mood]++;
    }
}
