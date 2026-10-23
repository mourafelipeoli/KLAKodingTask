namespace KodingTask.Api.Messaging;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string? Sender { get; set; }

    public string SenderName { get; set; } = "Koding Task";

    public string? Region { get; set; }

    public string? ConfigurationSet { get; set; }

    public string Mode { get; set; } = "Ses";

    public bool IsLogMode => string.Equals(Mode, "Log", StringComparison.OrdinalIgnoreCase);

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Sender) && (IsLogMode || !string.IsNullOrWhiteSpace(Region));
}
