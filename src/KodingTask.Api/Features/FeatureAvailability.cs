namespace KodingTask.Api.Features;

public sealed record FeatureAvailability(
    bool Email,
    bool WhatsApp,
    bool Ai,
    bool MachineTranslation,
    bool ExternalLogin)
{
    public static FeatureAvailability None { get; } = new(false, false, false, false, false);

    public bool Deliveries => Email || WhatsApp;

    public bool Accounts => ExternalLogin;
}
