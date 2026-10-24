using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Limits;

public sealed class UsageLimiter(KodingTaskDbContext database, TimeProvider timeProvider)
{
    public Task<bool> TryConsumeAsync(string counter, int limit, CancellationToken cancellationToken) =>
        TryConsumeAsync(counter, limit, 1, cancellationToken);

    public Task<bool> TryConsumeAsync(string counter, int limit, int amount, CancellationToken cancellationToken) =>
        TryConsumeAsync(Today(), counter, limit, amount, cancellationToken);

    public Task<bool> TryConsumeThisMonthAsync(string counter, int limit, CancellationToken cancellationToken)
    {
        var today = Today();
        return TryConsumeAsync(new DateOnly(today.Year, today.Month, 1), counter, limit, 1, cancellationToken);
    }

    private async Task<bool> TryConsumeAsync(DateOnly day, string counter, int limit, int amount, CancellationToken cancellationToken)
    {
        if (limit <= 0 || amount <= 0 || amount > limit)
            return false;

        if (await IncrementBelowLimitAsync(day, counter, limit, amount, cancellationToken))
            return true;

        if (await database.DailyUsage.AnyAsync(usage => usage.Day == day && usage.Counter == counter, cancellationToken))
            return false;

        database.DailyUsage.Add(new DailyUsage { Day = day, Counter = counter, Count = amount });
        try
        {
            await database.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            database.ChangeTracker.Clear();
            return await IncrementBelowLimitAsync(day, counter, limit, amount, cancellationToken);
        }
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    public async Task<int> CountAsync(string counter, CancellationToken cancellationToken)
    {
        var today = Today();
        return await database.DailyUsage
            .Where(usage => usage.Day == today && usage.Counter == counter)
            .Select(usage => usage.Count)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> IncrementBelowLimitAsync(DateOnly day, string counter, int limit, int amount, CancellationToken cancellationToken) =>
        await database.DailyUsage
            .Where(usage => usage.Day == day && usage.Counter == counter && usage.Count + amount <= limit)
            .ExecuteUpdateAsync(setters => setters.SetProperty(usage => usage.Count, usage => usage.Count + amount), cancellationToken) == 1;
}
