using KodingTask.Data.Entities;

namespace KodingTask.Api.Contracts;

public sealed record ConversionHistoryEntry(
    string Amount,
    int Dollars,
    int Cents,
    string Language,
    string Words,
    DateTimeOffset ConvertedAt)
{
    public static ConversionHistoryEntry From(ConversionRecord record) => new(
        record.Amount,
        record.Dollars,
        record.Cents,
        record.LanguageCode,
        record.Words,
        new DateTimeOffset(DateTime.SpecifyKind(record.ConvertedAtUtc, DateTimeKind.Utc)));
}
