namespace KodingTask.Api.Contracts;

public sealed record AntiforgeryResponse(string Token);

public sealed record AccountResponse(
    bool IsSignedIn,
    string? Email,
    bool EmailConfirmed,
    string? WhatsAppNumber,
    int? DeliveriesLeftToday)
{
    public static AccountResponse SignedOut { get; } = new(false, null, false, null, null);
}

public sealed record RegisterRequest(string? Email, string? Password, string? UiLocale, string? ReturnUrl);

public sealed record RegisterResponse(AccountResponse Account, bool ConfirmationSent);

public sealed record SignInRequest(string? Email, string? Password);

public sealed record ConfirmEmailRequest(string? UserId, string? Code);

public sealed record ResendConfirmationRequest(string? UiLocale, string? ReturnUrl);

public sealed record ForgotPasswordRequest(string? Email, string? UiLocale);

public sealed record ResetPasswordRequest(string? UserId, string? Code, string? Password);

public sealed record PhoneCodeRequest(string? Phone, string? UiLocale);

public sealed record PhoneCodeResponse(string MaskedNumber);

public sealed record PhoneVerifyRequest(string? Code);

public sealed record DeliveryRequest(string? Channel, string? Amount, string? Language, string? UiLocale);

public sealed record DeliveryResponse(string MaskedDestination, int DeliveriesLeftToday);
