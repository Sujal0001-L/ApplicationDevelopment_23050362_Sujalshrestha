using SQLite;

namespace YourJournal.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100), NotNull]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(100), NotNull, Unique]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(255), NotNull]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(6)]
    public string? PIN { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
