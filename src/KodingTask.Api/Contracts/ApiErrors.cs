namespace KodingTask.Api.Contracts;

public static class ApiErrors
{
    public static ErrorResponse MessagingUnavailable { get; } = new("MessagingUnavailable", "This feature is not available on this instance.");

    public static ErrorResponse Paused { get; } = new("Paused", "Sending is paused right now.");

    public static ErrorResponse DailyLimitReached { get; } = new("DailyLimitReached", "Today's sending limit has been reached. Try again tomorrow.");

    public static ErrorResponse MonthlyLimitReached { get; } = new("MonthlyLimitReached", "This month's WhatsApp limit has been reached. Send it by email instead.");

    public static ErrorResponse UserLimitReached { get; } = new("UserLimitReached", "You have reached your sending limit for today.");

    public static ErrorResponse SendFailed { get; } = new("SendFailed", "The message could not be sent. Try again later.");

    public static ErrorResponse EmailNotConfirmed { get; } = new("EmailNotConfirmed", "Confirm your email address first.");

    public static ErrorResponse PhoneNotVerified { get; } = new("PhoneNotVerified", "Add and confirm your WhatsApp number in your fmID account first.");

    public static ErrorResponse PhoneCheckNeeded { get; } = new("PhoneCheckNeeded", "Your WhatsApp number needs a fresh check with fmID.");

    public static ErrorResponse InvalidChannel { get; } = new("InvalidChannel", "Choose email or WhatsApp.");

    public static ErrorResponse InvalidTerms { get; } = new("InvalidTerms", "Send between 1 and 20 texts of up to 300 characters each.");

    public static ErrorResponse TermLimitReached { get; } = new("TermLimitReached", "Too many new texts today. Try again tomorrow.");

    public static ErrorResponse MachineTranslationUnavailable { get; } = new("MachineTranslationUnavailable", "Machine translation is not configured on this instance.");

    public static ErrorResponse UnknownLocale { get; } = new("UnknownLocale", "This language is not supported.");

    public static ErrorResponse UnknownTerm { get; } = new("UnknownTerm", "This text does not exist.");

    public static ErrorResponse InvalidTranslation { get; } = new("InvalidTranslation", "Type a translation of up to 2000 characters.");

    public static int StatusOf(ErrorResponse error) => error.Error switch
    {
        "MessagingUnavailable" or "MachineTranslationUnavailable" => StatusCodes.Status503ServiceUnavailable,
        "TermLimitReached" => StatusCodes.Status429TooManyRequests,
        "UnknownLocale" or "UnknownTerm" => StatusCodes.Status404NotFound,
        "Paused" or "DailyLimitReached" or "MonthlyLimitReached" or "UserLimitReached" => StatusCodes.Status429TooManyRequests,
        "SendFailed" => StatusCodes.Status502BadGateway,
        "EmailNotConfirmed" or "PhoneNotVerified" => StatusCodes.Status403Forbidden,
        "PhoneCheckNeeded" => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest,
    };
}
