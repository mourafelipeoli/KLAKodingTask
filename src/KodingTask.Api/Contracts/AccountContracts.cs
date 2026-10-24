namespace KodingTask.Api.Contracts;

public sealed record AntiforgeryResponse(string Token);

public sealed record AccountResponse(
    bool IsSignedIn,
    string? Email,
    string? WhatsAppNumber,
    int? DeliveriesLeftToday)
{
    public static AccountResponse SignedOut { get; } = new(false, null, null, null);
}

public sealed record PhoneCodeRequest(string? Phone, string? UiLocale);

public sealed record PhoneCodeResponse(string MaskedNumber);

public sealed record PhoneVerifyRequest(string? Code);

public sealed record FmIdForm(string? ReturnUrl, string? UiLocale);

public sealed record DeliveryRequest(string? Channel, string? Amount, string? Language, string? UiLocale);

public sealed record DeliveryResponse(string MaskedDestination, int DeliveriesLeftToday);
