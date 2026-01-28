namespace YourJournal.Models;

public static class TagData
{
    public static List<string> PrebuiltTags => new()
    {
        "Work", "Career", "Studies", "Family", "Friends", "Relationships",
        "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies",
        "Travel", "Nature", "Finance", "Spirituality", "Birthday", "Holiday",
        "Vacation", "Celebration", "Exercise", "Reading", "Writing", "Cooking",
        "Meditation", "Yoga", "Music", "Shopping", "Parenting", "Projects",
        "Planning", "Reflection"
    };
}

public static class Categories
{
    public static List<string> AllCategories => new()
    {
        "Personal Development",
        "Work & Career",
        "Health & Fitness",
        "Relationships",
        "Creative Writing",
        "Travel & Adventure",
        "Daily Reflection",
        "Gratitude",
        "Goals & Planning",
        "Dreams & Aspirations"
    };
}
