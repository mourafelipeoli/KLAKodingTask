using System.Diagnostics.CodeAnalysis;

namespace KodingTask.Conversion;

public sealed record ConversionResult
{
    private ConversionResult(string languageCode, MonetaryAmount amount, string? words, ConversionError? error)
    {
        LanguageCode = languageCode;
        Amount = amount;
        Words = words;
        Error = error;
    }

    public string LanguageCode { get; }

    public MonetaryAmount Amount { get; }

    public string? Words { get; }

    public ConversionError? Error { get; }

    [MemberNotNullWhen(true, nameof(Words))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static ConversionResult Succeeded(string languageCode, MonetaryAmount amount, string words) =>
        new(languageCode, amount, words, null);

    public static ConversionResult Failed(string languageCode, ConversionError error) =>
        new(languageCode, default, null, error);
}
