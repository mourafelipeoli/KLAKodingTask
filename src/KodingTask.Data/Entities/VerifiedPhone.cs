namespace KodingTask.Data.Entities;

public class VerifiedPhone
{
    public required string UserId { get; set; }

    public required string PhoneE164 { get; set; }

    public DateTime VerifiedAtUtc { get; set; }
}
