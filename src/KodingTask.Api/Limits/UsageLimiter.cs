using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Limits;

public sealed class UsageLimiter(KodingTaskDbContext database, TimeProvider timeProvider)
{
    public async Task<bool> TryConsumeAsync(string counter, int limit, CancellationToken cancellationToken)
    {
        if (limit <= 0)
            return false;

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        if (await IncrementBelowLimitAsync(today, counter, limit, cancellationToken))
            return true;

        if (await database.DailyUsage.AnyAsync(usage => usage.Day == today && usage.Counter == counter, cancellationToken))
            return false;

        database.DailyUsage.Add(new DailyUsage { Day = today, Counter = counter, Count = 1 });
        try
        {
            await database.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            database.ChangeTracker.Clear();
            return await IncrementBelowLimitAsync(today, counter, limit, cancellationToken);
        }
    }

    public async Task<int> CountAsync(string counter, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        return await database.DailyUsage
            .Where(usage => usage.Day == today && usage.Counter == counter)
            .Select(usage => usage.Count)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> IncrementBelowLimitAsync(DateOnly day, string counter, int limit, CancellationToken cancellationToken) =>
        await database.DailyUsage
            .Where(usage => usage.Day == day && usage.Counter == counter && usage.Count < limit)
            .ExecuteUpdateAsync(setters => setters.SetProperty(usage => usage.Count, usage => usage.Count + 1), cancellationToken) == 1;
}
