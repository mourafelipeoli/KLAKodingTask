namespace KodingTask.Api.Contracts;

public static class ApiErrors
{
    public static ErrorResponse MessagingUnavailable { get; } = new("MessagingUnavailable", "This feature is not available on this instance.");

    public static ErrorResponse Paused { get; } = new("Paused", "Sending is paused right now.");

    public static ErrorResponse DailyLimitReached { get; } = new("DailyLimitReached", "Today's sending limit has been reached. Try again tomorrow.");

    public static ErrorResponse UserLimitReached { get; } = new("UserLimitReached", "You have reached your sending limit for today.");

    public static ErrorResponse SendFailed { get; } = new("SendFailed", "The message could not be sent. Try again later.");

    public static ErrorResponse EmailNotConfirmed { get; } = new("EmailNotConfirmed", "Confirm your email address first.");

    public static ErrorResponse PhoneNotVerified { get; } = new("PhoneNotVerified", "Add and confirm your WhatsApp number in your fmID account first.");

    public static ErrorResponse PhoneCheckNeeded { get; } = new("PhoneCheckNeeded", "Your WhatsApp number needs a fresh check with fmID.");

    public static ErrorResponse InvalidChannel { get; } = new("InvalidChannel", "Choose email or WhatsApp.");

    public static int StatusOf(ErrorResponse error) => error.Error switch
    {
        "MessagingUnavailable" => StatusCodes.Status503ServiceUnavailable,
        "Paused" or "DailyLimitReached" or "UserLimitReached" => StatusCodes.Status429TooManyRequests,
        "SendFailed" => StatusCodes.Status502BadGateway,
        "EmailNotConfirmed" or "PhoneNotVerified" => StatusCodes.Status403Forbidden,
        "PhoneCheckNeeded" => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest,
    };
}
