using SQLite;

namespace MoodJournal.Models;

public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    //Format: yyyy-MM-dd
    [Indexed(Name = "IX_JournalEntry_EntryDate", Unique = true)]
    public string EntryDate { get; set; } = "";

    public string Title { get; set; } = "";
    
    public string ContentHtml { get; set; } = "";
    
    public string ContentText { get; set; } = "";

    public int WordCount { get; set; }

    //required
    public string PrimaryMood { get; set; } = "";

    //optional (1-2)
    public string SecondaryMood1 { get; set; } = "";
    public string SecondaryMood2 { get; set; } = "";

    //optional
    public string Category { get; set; } = "";
    
    public string TagsCsv { get; set; } = "";
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}