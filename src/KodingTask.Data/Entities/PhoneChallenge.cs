namespace KodingTask.Data.Entities;

public class PhoneChallenge
{
    public required string UserId { get; set; }

    public required string PhoneE164 { get; set; }

    public required string CodeHash { get; set; }

    public DateTime SentAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public int FailedAttempts { get; set; }
}
