using SQLite;
using MoodJournal.Models;

namespace MoodJournal.Services;

public class JournalRepository
{
    private readonly SQLiteAsyncConnection _db;

    public JournalRepository()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "moodjournal.db");
        _db = new SQLiteAsyncConnection(dbPath);

        //checks if table exists
        _ = _db.CreateTableAsync<JournalEntry>();
    }

    public async Task<JournalEntry?> GetByDateAsync(DateTime date)
    {
        var key = date.ToString("yyyy-MM-dd");

        return await _db.Table<JournalEntry>()
            .Where(x => x.EntryDate == key)
            .FirstOrDefaultAsync();
    }

    public async Task<List<JournalEntry>> GetAllAsync()
    {
        return await _db.Table<JournalEntry>()
            .OrderByDescending(x => x.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetForMonthAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
        var end = new DateTime(year, month, 1).AddMonths(1).ToString("yyyy-MM-dd");

        return await _db.Table<JournalEntry>()
            .Where(x => x.EntryDate.CompareTo(start) >= 0 && x.EntryDate.CompareTo(end) < 0)
            .OrderBy(x => x.EntryDate)
            .ToListAsync();
    }

    public async Task DeleteByDateAsync(DateTime date)
    {
        var key = date.ToString("yyyy-MM-dd");

        var existing = await _db.Table<JournalEntry>()
            .Where(x => x.EntryDate == key)
            .FirstOrDefaultAsync();

        if (existing is null) return;

        await _db.DeleteAsync(existing);
    }

    public async Task SaveOrUpdateTodayAsync(
        DateTime date,
        string title,
        string contentHtml,
        string contentText,
        int wordCount,
        string primaryMood,
        List<string> secondaryMoods,
        string category,
        List<string> tags)
    {
        var key = date.ToString("yyyy-MM-dd");
        var now = DateTime.Now;

        var s1 = secondaryMoods.Count > 0 ? secondaryMoods[0] : "";
        var s2 = secondaryMoods.Count > 1 ? secondaryMoods[1] : "";

        var tagsCsv = string.Join(",", tags);

        var existing = await _db.Table<JournalEntry>()
            .Where(x => x.EntryDate == key)
            .FirstOrDefaultAsync();

        if (existing is null)
        {
            var entry = new JournalEntry
            {
                EntryDate = key,
                Title = title ?? "",
                ContentHtml = contentHtml ?? "",
                ContentText = contentText ?? "",
                WordCount = wordCount,
                PrimaryMood = primaryMood ?? "",
                SecondaryMood1 = s1,
                SecondaryMood2 = s2,
                Category = category ?? "",
                TagsCsv = tagsCsv ?? "",
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.InsertAsync(entry);
            return;
        }

        existing.Title = title ?? "";
        existing.ContentHtml = contentHtml ?? "";
        existing.ContentText = contentText ?? "";
        existing.WordCount = wordCount;
        existing.PrimaryMood = primaryMood ?? "";
        existing.SecondaryMood1 = s1;
        existing.SecondaryMood2 = s2;
        existing.Category = category ?? "";
        existing.TagsCsv = tagsCsv ?? "";
        existing.UpdatedAt = now;

        await _db.UpdateAsync(existing);
    }
}
