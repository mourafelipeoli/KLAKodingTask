namespace KodingTask.Data.Entities;

public class UserProfile
{
    public required string UserId { get; set; }

    public required string PreferredLocale { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
