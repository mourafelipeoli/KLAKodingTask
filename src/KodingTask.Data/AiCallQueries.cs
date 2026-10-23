using Microsoft.EntityFrameworkCore;

namespace KodingTask.Data;

public static class AiCallQueries
{
    public static async Task<long> MonthToDateMicroUsdAsync(this KodingTaskDbContext database, DateTime utcNow, CancellationToken cancellationToken)
    {
        var monthStart = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return await database.AiCalls
            .Where(call => call.CreatedAtUtc >= monthStart)
            .SumAsync(call => (long?)call.CostMicroUsd, cancellationToken) ?? 0;
    }
}
