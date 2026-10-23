using KodingTask.Conversion;

namespace KodingTask.Api.Contracts;

public sealed record ConversionResponse(
    string Amount,
    string NormalizedAmount,
    int Dollars,
    int Cents,
    string Language,
    string Words,
    string Engine)
{
    public const string DeterministicEngine = "Deterministic";

    public static ConversionResponse From(string input, ConversionResult result)
    {
        if (!result.IsSuccess)
            throw new ArgumentException("Only successful conversions have a response.", nameof(result));

        return new(
            input.Trim(),
            result.Amount.ToCanonicalString(),
            result.Amount.Dollars,
            result.Amount.Cents,
            result.LanguageCode,
            result.Words,
            DeterministicEngine);
    }
}
