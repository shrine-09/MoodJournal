using SQLite;

namespace MoodJournal.Models;

public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Use DateOnly-like storage but keep string for easy SQLite querying
    // Format: yyyy-MM-dd
    [Indexed(Name = "IX_JournalEntry_EntryDate", Unique = true)]
    public string EntryDate { get; set; } = "";

    public string Title { get; set; } = "";

    // Store rich text as HTML (from Quill)
    public string ContentHtml { get; set; } = "";

    // Store plain text for analytics/word count
    public string ContentText { get; set; } = "";

    public int WordCount { get; set; }

    // Required
    public string PrimaryMood { get; set; } = "";

    // Optional (up to 2)
    public string SecondaryMood1 { get; set; } = "";
    public string SecondaryMood2 { get; set; } = "";

    // Optional
    public string Category { get; set; } = "";

    // Comma-separated tags (simple for coursework)
    public string TagsCsv { get; set; } = "";

    // System generated
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}