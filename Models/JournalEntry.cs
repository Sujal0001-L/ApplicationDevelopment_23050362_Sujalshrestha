using SQLite;

namespace YourJournal.Models;

[Table("JournalEntries")]
public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [NotNull]
    public int UserId { get; set; }
    
    [NotNull]
    public DateTime Date { get; set; } = DateTime.Today;
    
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [NotNull]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(50), NotNull]
    public string PrimaryMood { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? SecondaryMood1 { get; set; }
    
    [MaxLength(50)]
    public string? SecondaryMood2 { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    public string? Tags { get; set; } // Stored as JSON array
    
    public int WordCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
