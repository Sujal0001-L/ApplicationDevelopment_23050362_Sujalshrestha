namespace YourJournal.Models;

public class Mood
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Positive, Neutral, Negative
    public string Icon { get; set; } = string.Empty;
}

public static class MoodData
{
    public static List<Mood> AllMoods => new()
    {
        // Positive
        new Mood { Name = "Happy", Category = "Positive", Icon = "😊" },
        new Mood { Name = "Excited", Category = "Positive", Icon = "🤩" },
        new Mood { Name = "Relaxed", Category = "Positive", Icon = "😌" },
        new Mood { Name = "Grateful", Category = "Positive", Icon = "🙏" },
        new Mood { Name = "Confident", Category = "Positive", Icon = "💪" },
        
        // Neutral
        new Mood { Name = "Calm", Category = "Neutral", Icon = "😐" },
        new Mood { Name = "Thoughtful", Category = "Neutral", Icon = "🤔" },
        new Mood { Name = "Curious", Category = "Neutral", Icon = "🧐" },
        new Mood { Name = "Nostalgic", Category = "Neutral", Icon = "📸" },
        new Mood { Name = "Bored", Category = "Neutral", Icon = "😑" },
        
        // Negative
        new Mood { Name = "Sad", Category = "Negative", Icon = "😢" },
        new Mood { Name = "Angry", Category = "Negative", Icon = "😠" },
        new Mood { Name = "Stressed", Category = "Negative", Icon = "😰" },
        new Mood { Name = "Lonely", Category = "Negative", Icon = "😔" },
        new Mood { Name = "Anxious", Category = "Negative", Icon = "😟" }
    };
    
    public static List<string> PositiveMoods => AllMoods.Where(m => m.Category == "Positive").Select(m => m.Name).ToList();
    public static List<string> NeutralMoods => AllMoods.Where(m => m.Category == "Neutral").Select(m => m.Name).ToList();
    public static List<string> NegativeMoods => AllMoods.Where(m => m.Category == "Negative").Select(m => m.Name).ToList();
}
