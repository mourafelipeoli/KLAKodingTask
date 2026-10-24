using KodingTask.Api.Contracts;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.History;

public sealed class ConversionHistory(KodingTaskDbContext database, TimeProvider timeProvider, ILogger<ConversionHistory> logger)
{
    public const int Size = 10;

    public async Task RecordAsync(string userId, ConversionResponse conversion, CancellationToken cancellationToken)
    {
        try
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var record = await database.ConversionRecords.SingleOrDefaultAsync(
                row => row.UserId == userId && row.Amount == conversion.NormalizedAmount && row.LanguageCode == conversion.Language,
                cancellationToken);
            if (record is null)
            {
                database.ConversionRecords.Add(new ConversionRecord
                {
                    UserId = userId,
                    Amount = conversion.NormalizedAmount,
                    Dollars = conversion.Dollars,
                    Cents = conversion.Cents,
                    LanguageCode = conversion.Language,
                    Words = conversion.Words,
                    ConvertedAtUtc = now,
                });
            }
            else
            {
                record.Words = conversion.Words;
                record.ConvertedAtUtc = now;
            }

            await database.SaveChangesAsync(cancellationToken);

            var oldestKept = await Newest(userId)
                .Skip(Size - 1)
                .Select(row => (DateTime?)row.ConvertedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
            if (oldestKept is not null)
            {
                await database.ConversionRecords
                    .Where(row => row.UserId == userId && row.ConvertedAtUtc < oldestKept)
                    .ExecuteDeleteAsync(cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            database.ChangeTracker.Clear();
            logger.LogWarning("[History] Could not record a conversion: {Error}.", exception.GetType().Name);
        }
    }

    public async Task<IReadOnlyList<ConversionHistoryEntry>> ListAsync(string userId, CancellationToken cancellationToken)
    {
        var records = await Newest(userId).AsNoTracking().Take(Size).ToListAsync(cancellationToken);
        return records.Select(ConversionHistoryEntry.From).ToList();
    }

    public Task<int> ClearAsync(string userId, CancellationToken cancellationToken) =>
        database.ConversionRecords.Where(row => row.UserId == userId).ExecuteDeleteAsync(cancellationToken);

    private IQueryable<ConversionRecord> Newest(string userId) =>
        database.ConversionRecords
            .Where(row => row.UserId == userId)
            .OrderByDescending(row => row.ConvertedAtUtc)
            .ThenByDescending(row => row.Id);
}
