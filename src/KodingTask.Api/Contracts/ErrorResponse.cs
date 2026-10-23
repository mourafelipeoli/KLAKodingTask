using KodingTask.Conversion;

namespace KodingTask.Api.Contracts;

public sealed record ErrorResponse(string Error, string Message)
{
    private static readonly Dictionary<ConversionError, string> ConversionMessages = new()
    {
        [ConversionError.Empty] = "Type an amount to convert.",
        [ConversionError.TooLong] = "The amount is too long.",
        [ConversionError.InvalidCharacter] = "Only digits, spaces and one comma are allowed.",
        [ConversionError.NegativeAmount] = "Negative amounts are not supported.",
        [ConversionError.DecimalPointNotAllowed] = "Use a comma, not a point, to separate the cents.",
        [ConversionError.MultipleCommas] = "Use only one comma, to separate the cents.",
        [ConversionError.MissingDollars] = "Type the dollars before the comma, for example 0,50.",
        [ConversionError.InvalidCents] = "After the comma, type one or two digits for the cents.",
        [ConversionError.TooManyCentDigits] = "Cents can have at most two digits.",
        [ConversionError.InvalidGrouping] = "Group thousands in blocks of three digits, for example 45 100.",
        [ConversionError.DollarsOutOfRange] = "The maximum is 999 999 999 dollars.",
        [ConversionError.UnsupportedLanguage] = "This language is not supported.",
    };

    public static ErrorResponse TooManyRequests { get; } =
        new("TooManyRequests", "Too many requests in a short time. Wait a minute and try again.");

    public static ErrorResponse NotFound { get; } =
        new("NotFound", "There is no API endpoint at this address.");

    public static ErrorResponse For(ConversionError error) => new(error.ToString(), ConversionMessages[error]);
}
