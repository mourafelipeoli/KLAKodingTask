using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Infrastructure;

public sealed record InfrastructureSnapshot(DateTimeOffset TakenAt, HostingFacts Hosting, AwsSnapshot Aws);

public sealed class InfrastructureSnapshotSource(
    ITaskMetadataSource metadataSource,
    IAwsInfrastructureProbe probe,
    IOptions<InfrastructureOptions> options,
    TimeProvider timeProvider,
    ILogger<InfrastructureSnapshotSource> logger)
{
    private static readonly DateTimeOffset ProcessStartedAt = new(Process.GetCurrentProcess().StartTime.ToUniversalTime());

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private InfrastructureSnapshot? _cached;
    private DateTimeOffset _expiresAt;

    public async Task<InfrastructureSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        if (_cached is not null && timeProvider.GetUtcNow() < _expiresAt)
            return _cached;

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_cached is not null && timeProvider.GetUtcNow() < _expiresAt)
                return _cached;

            _cached = await TakeAsync();
            _expiresAt = timeProvider.GetUtcNow().AddMinutes(Math.Max(1, options.Value.CacheMinutes));
            return _cached;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<InfrastructureSnapshot> TakeAsync()
    {
        var metadata = await metadataSource.ReadAsync(CancellationToken.None);
        var aws = AwsSnapshot.Empty;
        if (metadata is not null)
        {
            try
            {
                aws = await probe.ProbeAsync(metadata, CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogWarning("[Infrastructure] The AWS facts are unavailable: {Error}.", exception.GetType().Name);
            }
        }

        return new InfrastructureSnapshot(timeProvider.GetUtcNow(), Hosting(metadata), aws);
    }

    internal static HostingFacts Hosting(TaskMetadata? metadata) => new(
        OnAws: metadata is not null,
        Region: metadata?.Region,
        RegionName: AwsRegionNames.NameOf(metadata?.Region),
        AvailabilityZone: metadata?.AvailabilityZone,
        Compute: metadata?.LaunchType?.ToUpperInvariant() switch
        {
            null => null,
            "FARGATE" => "AWS Fargate",
            "EC2" => "Amazon EC2",
            _ => "Amazon ECS",
        },
        Vcpu: metadata?.Limits?.Cpu,
        MemoryMiB: metadata?.Limits?.Memory,
        StartedAt: metadata?.StartedAt ?? ProcessStartedAt,
        Runtime: RuntimeInformation.FrameworkDescription,
        OperatingSystem: RuntimeInformation.OSDescription,
        Architecture: RuntimeInformation.OSArchitecture.ToString());
}
