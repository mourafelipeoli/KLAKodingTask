namespace KodingTask.Api.Contracts;

public static class ApiErrors
{
    public static ErrorResponse MessagingUnavailable { get; } = new("MessagingUnavailable", "This feature is not available on this instance.");

    public static ErrorResponse Paused { get; } = new("Paused", "Sending is paused right now.");

    public static ErrorResponse DailyLimitReached { get; } = new("DailyLimitReached", "Today's sending limit has been reached. Try again tomorrow.");

    public static ErrorResponse UserLimitReached { get; } = new("UserLimitReached", "You have reached your sending limit for today.");

    public static ErrorResponse SendFailed { get; } = new("SendFailed", "The message could not be sent. Try again later.");

    public static ErrorResponse EmailNotConfirmed { get; } = new("EmailNotConfirmed", "Confirm your email address first.");

    public static ErrorResponse PhoneNotVerified { get; } = new("PhoneNotVerified", "Verify your WhatsApp number first.");

    public static ErrorResponse InvalidPhone { get; } = new("InvalidPhone", "Type the number in international format, for example +49 170 1234567.");

    public static ErrorResponse CodeCooldown { get; } = new("CodeCooldown", "Wait a minute before asking for a new code.");

    public static ErrorResponse NoPendingCode { get; } = new("NoPendingCode", "Ask for a code first.");

    public static ErrorResponse CodeExpired { get; } = new("CodeExpired", "The code has expired. Ask for a new one.");

    public static ErrorResponse WrongCode { get; } = new("WrongCode", "The code is not right.");

    public static ErrorResponse TooManyAttempts { get; } = new("TooManyAttempts", "Too many wrong codes. Ask for a new one.");

    public static ErrorResponse InvalidChannel { get; } = new("InvalidChannel", "Choose email or WhatsApp.");

    public static int StatusOf(ErrorResponse error) => error.Error switch
    {
        "MessagingUnavailable" => StatusCodes.Status503ServiceUnavailable,
        "Paused" or "DailyLimitReached" or "UserLimitReached" or "CodeCooldown" => StatusCodes.Status429TooManyRequests,
        "SendFailed" => StatusCodes.Status502BadGateway,
        "EmailNotConfirmed" or "PhoneNotVerified" => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest,
    };
}
