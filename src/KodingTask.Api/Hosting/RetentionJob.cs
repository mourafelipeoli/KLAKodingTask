using KodingTask.Data;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Hosting;

public sealed class RetentionJob(IServiceScopeFactory scopes, TimeProvider timeProvider, ILogger<RetentionJob> logger) : BackgroundService
{
    public static readonly TimeSpan Retention = TimeSpan.FromDays(90);
    public static readonly TimeSpan CounterRetention = TimeSpan.FromDays(40);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), timeProvider, stoppingToken);
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24), timeProvider);
            do
            {
                try
                {
                    var removed = await PurgeAsync(stoppingToken);
                    logger.LogInformation("[Retention] Removed {Rows} expired rows.", removed);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogWarning("[Retention] The purge failed: {Error}.", exception.GetType().Name);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    internal async Task<int> PurgeAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cutoff = now - Retention;
        var counterCutoff = DateOnly.FromDateTime(now - CounterRetention);

        return await database.OutboundMessages.Where(message => message.CreatedAtUtc < cutoff).ExecuteDeleteAsync(cancellationToken)
            + await database.AiCalls.Where(call => call.CreatedAtUtc < cutoff).ExecuteDeleteAsync(cancellationToken)
            + await database.ConversionRecords.Where(record => record.ConvertedAtUtc < cutoff).ExecuteDeleteAsync(cancellationToken)
            + await database.DailyUsage.Where(usage => usage.Day < counterCutoff).ExecuteDeleteAsync(cancellationToken);
    }
}
