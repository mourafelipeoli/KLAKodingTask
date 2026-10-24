namespace KodingTask.Data.Entities;

public class OutboundMessage
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? UserId { get; set; }

    public MessageChannel Channel { get; set; }

    public MessageKind Kind { get; set; }

    public required string MaskedDestination { get; set; }

    public MessageStatus Status { get; set; }

    public string? ProviderMessageId { get; set; }

    public string? Error { get; set; }
}

public enum MessageChannel
{
    Email,
    WhatsApp,
}

public enum MessageKind
{
    Result,
}

public enum MessageStatus
{
    Sent,
    Failed,
}
