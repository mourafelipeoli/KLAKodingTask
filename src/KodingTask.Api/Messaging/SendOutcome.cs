using KodingTask.Api.Contracts;

namespace KodingTask.Api.Messaging;

public sealed record SendOutcome(string? MaskedDestination, ErrorResponse? Error)
{
    public bool IsSent => Error is null;

    public static SendOutcome Sent(string maskedDestination) => new(maskedDestination, null);

    public static SendOutcome Refused(ErrorResponse error) => new(null, error);
}
