namespace KodingTask.Api.Contracts;

public sealed record AntiforgeryResponse(string Token);

public sealed record AccountResponse(
    bool IsSignedIn,
    string? Email,
    string? WhatsAppNumber,
    int? DeliveriesLeftToday,
    bool IsAdmin)
{
    public static AccountResponse SignedOut { get; } = new(false, null, null, null, false);
}

public sealed record FmIdForm(string? ReturnUrl, string? UiLocale);

public sealed record DeliveryRequest(string? Channel, string? Amount, string? Language, string? UiLocale);

public sealed record DeliveryResponse(string MaskedDestination, int DeliveriesLeftToday);
