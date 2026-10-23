using KodingTask.Api.Ai;
using KodingTask.Api.Features;
using KodingTask.Api.Hosting;
using KodingTask.Api.Limits;
using KodingTask.Data;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Infrastructure;

public sealed class InfrastructureReporter(
    InfrastructureSnapshotSource snapshots,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    KodingTaskDbContext database,
    DatabaseOptions databaseOptions,
    IOptions<LimitsOptions> limitsOptions,
    IOptions<PublicSiteOptions> publicSite,
    IConfiguration configuration,
    FeatureAvailability features,
    TimeProvider timeProvider)
{
    public async Task<InfrastructureReport> BuildAsync(bool https, CancellationToken cancellationToken)
    {
        var snapshot = await snapshots.GetAsync(cancellationToken);
        var limits = await limitsSource.GetAsync(cancellationToken);
        var aiCallsToday = await limiter.CountAsync(AiConversionService.DailyCounter, cancellationToken);
        var spentMicroUsd = await database.MonthToDateMicroUsdAsync(timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        var emailsToday = await limiter.CountAsync(Counters.Emails, cancellationToken);
        var whatsAppToday = await limiter.CountAsync(Counters.WhatsAppMessages, cancellationToken);

        return new InfrastructureReport(
            snapshot.TakenAt,
            snapshot.Hosting,
            snapshot.Aws.Service,
            snapshot.Aws.Traffic,
            Build(),
            Data(snapshot),
            new SecurityFacts(https, snapshot.Aws.CertificateValidUntil, !string.IsNullOrWhiteSpace(limitsOptions.Value.SsmPath)),
            new GuardrailFacts(
                limits.KillSwitch,
                aiCallsToday,
                limits.AiCallsPerDay,
                spentMicroUsd / 1_000_000m,
                limits.AiMonthlyBudgetUsd,
                emailsToday,
                limits.EmailsPerDay,
                whatsAppToday,
                limits.WhatsAppPerDay),
            features);
    }

    private BuildFacts Build()
    {
        var commit = configuration.Commit();
        var repository = publicSite.Value.RepositoryUrl;
        var commitUrl = string.IsNullOrWhiteSpace(commit) || string.IsNullOrWhiteSpace(repository)
            ? null
            : $"{repository.TrimEnd('/')}/commit/{commit}";
        return new BuildFacts(configuration.Version(), commit, commitUrl, configuration[ApplicationVersion.BuildDateKey]);
    }

    private DataFacts Data(InfrastructureSnapshot snapshot)
    {
        if (databaseOptions.Provider == DatabaseProvider.Sqlite)
            return new DataFacts("SQLite", null, null, Persistent: !snapshot.Hosting.OnAws, MultiAz: null);

        return snapshot.Aws.Database is { } managed
            ? new DataFacts(managed.Engine, managed.Version, "Amazon RDS", Persistent: true, managed.MultiAz)
            : new DataFacts("MySQL", null, null, Persistent: true, MultiAz: null);
    }
}
