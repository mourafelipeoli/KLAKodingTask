namespace KodingTask.Api.Messaging;

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    public string ApiBaseUrl { get; set; } = "https://graph.facebook.com/v22.0";

    public string? PhoneNumberId { get; set; }

    public string? AccessToken { get; set; }

    public string ResultTemplate { get; set; } = "koding_task_result";

    public string CodeTemplate { get; set; } = "koding_task_code";

    public string FallbackLanguage { get; set; } = "en_US";

    public int TimeoutSeconds { get; set; } = 15;

    public string Mode { get; set; } = "Meta";

    public bool IsLogMode => string.Equals(Mode, "Log", StringComparison.OrdinalIgnoreCase);

    public bool IsConfigured => IsLogMode || (
        !string.IsNullOrWhiteSpace(PhoneNumberId)
        && !string.IsNullOrWhiteSpace(AccessToken)
        && !string.IsNullOrWhiteSpace(ResultTemplate)
        && !string.IsNullOrWhiteSpace(CodeTemplate));
}
