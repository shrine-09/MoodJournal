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
    /*USING THIS TO Add 2weeks of fake entries
    public async Task SeedDemoLastTwoWeeksAsync()
    {
        // Creates entries for the last 14 days (including today),
        // skipping 1 day in the middle. Won't overwrite existing days.

        var moods = new[]
        {
            "Happy","Excited","Relaxed","Grateful","Confident",
            "Calm","Thoughtful","Curious","Nostalgic","Bored",
            "Sad","Angry","Stressed","Lonely","Anxious"
        };

        var positive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Happy","Excited","Relaxed","Grateful","Confident" };

        var neutral = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Calm","Thoughtful","Curious","Nostalgic","Bored" };

        var negative = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Sad","Angry","Stressed","Lonely","Anxious" };

        var tagsPool = new[]
        {
            "Work","Studies","Family","Friends","Health","Fitness",
            "Personal Growth","Self-care","Hobbies","Travel","Nature",
            "Finance","Projects","Planning","Reflection","Reading","Music"
        };

        var today = DateTime.Today;
        var start = today.AddDays(-13);

        // One missing day in-between (example: 6 days ago)
        var missing = today.AddDays(-6);

        for (int i = 0; i < 14; i++)
        {
            var d = start.AddDays(i);

            if (d == missing)
                continue;

            // don't overwrite existing entries
            var existing = await GetByDateAsync(d);
            if (existing != null)
                continue;

            // stable random per day (so it looks consistent)
            var rng = new Random(d.GetHashCode());

            var primaryMood = moods[rng.Next(moods.Length)];

            var category =
                positive.Contains(primaryMood) ? "Positive" :
                negative.Contains(primaryMood) ? "Negative" :
                neutral.Contains(primaryMood) ? "Neutral" : "Neutral";

            // secondary moods (0-2, different from primary)
            var secondary = new List<string>();
            if (rng.NextDouble() < 0.55)
            {
                var s1 = moods[rng.Next(moods.Length)];
                if (!s1.Equals(primaryMood, StringComparison.OrdinalIgnoreCase))
                    secondary.Add(s1);
            }
            if (rng.NextDouble() < 0.30)
            {
                var s2 = moods[rng.Next(moods.Length)];
                if (!s2.Equals(primaryMood, StringComparison.OrdinalIgnoreCase) &&
                    !secondary.Any(x => x.Equals(s2, StringComparison.OrdinalIgnoreCase)))
                    secondary.Add(s2);
            }

            // tags (1-3)
            var tags = new List<string>();
            var tagCount = rng.Next(1, 4);
            while (tags.Count < tagCount)
            {
                var t = tagsPool[rng.Next(tagsPool.Length)];
                if (!tags.Any(x => x.Equals(t, StringComparison.OrdinalIgnoreCase)))
                    tags.Add(t);
            }

            var title = $"Day {d:MMM d} check-in";

            var contentText =
                $"Today felt {primaryMood.ToLowerInvariant()}. " +
                $"Main focus: {tags[0]}. " +
                $"Small wins and notes for the day.";

            var contentHtml =
                $"<p>{System.Net.WebUtility.HtmlEncode(contentText)}</p>";

            var wordCount = contentText
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length;

            await SaveOrUpdateTodayAsync(
                d,
                title,
                contentHtml,
                contentText,
                wordCount,
                primaryMood,
                secondary,
                category,
                tags
            );
        }
    }
    */

}

