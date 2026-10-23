using Amazon;
using Amazon.CertificateManager;
using Amazon.CertificateManager.Model;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.ECS;
using DescribeServicesRequest = Amazon.ECS.Model.DescribeServicesRequest;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Amazon.RDS;
using DescribeDBInstancesRequest = Amazon.RDS.Model.DescribeDBInstancesRequest;
using Amazon.Runtime;
using KodingTask.Data;
using Microsoft.Extensions.Options;
using Metric = Amazon.CloudWatch.Model.Metric;
using MetricDimension = Amazon.CloudWatch.Model.Dimension;

namespace KodingTask.Api.Infrastructure;

public interface IAwsInfrastructureProbe
{
    Task<AwsSnapshot> ProbeAsync(TaskMetadata metadata, CancellationToken cancellationToken);
}

public sealed record AwsSnapshot(
    ServiceFacts? Service,
    TrafficFacts? Traffic,
    DateTimeOffset? CertificateValidUntil,
    ManagedDatabase? Database)
{
    public static AwsSnapshot Empty { get; } = new(null, null, null, null);
}

public sealed record ManagedDatabase(string Engine, string? Version, bool? MultiAz);

public sealed class AwsInfrastructureProbe(
    IOptions<InfrastructureOptions> options,
    DatabaseOptions database,
    TimeProvider timeProvider,
    ILogger<AwsInfrastructureProbe> logger) : IAwsInfrastructureProbe, IDisposable
{
    private readonly Lock _clientsLock = new();
    private AwsClients? _clients;

    public async Task<AwsSnapshot> ProbeAsync(TaskMetadata metadata, CancellationToken cancellationToken)
    {
        if (metadata.Region is not { } region)
            return AwsSnapshot.Empty;

        var settings = options.Value;
        var clients = ClientsFor(region, settings.AwsCallTimeoutSeconds);

        var certificate = Safe("certificate", token => CertificateValidUntilAsync(clients, settings.CertificateArn, token), cancellationToken);
        var managedDatabase = Safe("database", token => ManagedDatabaseAsync(clients, settings.DatabaseInstanceId, token), cancellationToken);
        var service = await Safe("service", token => DescribeServiceAsync(clients, metadata, token), cancellationToken);
        var targets = Safe("target health", token => TargetCountsAsync(clients, service?.TargetGroupArn, token), cancellationToken);
        var traffic = Safe("metrics", token => TrafficAsync(clients, metadata, service?.TargetGroupArn, token), cancellationToken);
        await Task.WhenAll(certificate, managedDatabase, targets, traffic);

        var serviceFacts = service is null
            ? null
            : service.Facts with { HealthyTargets = targets.Result?.Healthy, RegisteredTargets = targets.Result?.Registered };
        return new AwsSnapshot(serviceFacts, traffic.Result, certificate.Result, managedDatabase.Result);
    }

    public void Dispose() => _clients?.Dispose();

    private async Task<T> Safe<T>(string what, Func<CancellationToken, Task<T>> call, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, options.Value.AwsCallTimeoutSeconds)));
        try
        {
            return await call(timeout.Token);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            var reason = exception is AmazonServiceException service ? service.ErrorCode : exception.GetType().Name;
            logger.LogWarning("[Infrastructure] Could not read the {What}: {Reason}.", what, reason);
            return default!;
        }
    }

    private AwsClients ClientsFor(string region, int timeoutSeconds)
    {
        lock (_clientsLock)
        {
            if (_clients?.Region != region)
            {
                _clients?.Dispose();
                _clients = new AwsClients(region, TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));
            }

            return _clients;
        }
    }

    private static async Task<DescribedService?> DescribeServiceAsync(AwsClients clients, TaskMetadata metadata, CancellationToken cancellationToken)
    {
        if (metadata.ClusterName is not { } cluster || string.IsNullOrWhiteSpace(metadata.ServiceName))
            return null;

        var response = await clients.Ecs.DescribeServicesAsync(new DescribeServicesRequest
        {
            Cluster = cluster,
            Services = [metadata.ServiceName],
        }, cancellationToken);
        var service = response.Services?.FirstOrDefault();
        if (service is null)
            return null;

        var primary = service.Deployments?.FirstOrDefault(deployment => deployment.Status == "PRIMARY");
        var facts = new ServiceFacts(
            Running: service.RunningCount ?? 0,
            Desired: service.DesiredCount ?? 0,
            HealthyTargets: null,
            RegisteredTargets: null,
            RolloutState: primary?.RolloutState?.Value,
            LastDeployedAt: AsUtc(primary?.CreatedAt),
            AutomaticRollback: service.DeploymentConfiguration?.DeploymentCircuitBreaker?.Rollback == true,
            PublicIpAddress: service.NetworkConfiguration?.AwsvpcConfiguration?.AssignPublicIp == AssignPublicIp.ENABLED);
        return new DescribedService(facts, service.LoadBalancers?.FirstOrDefault()?.TargetGroupArn);
    }

    private static async Task<TargetCounts?> TargetCountsAsync(AwsClients clients, string? targetGroupArn, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(targetGroupArn))
            return null;

        var response = await clients.LoadBalancing.DescribeTargetHealthAsync(new DescribeTargetHealthRequest
        {
            TargetGroupArn = targetGroupArn,
        }, cancellationToken);
        var targets = response.TargetHealthDescriptions ?? [];
        return new TargetCounts(
            targets.Count(target => target.TargetHealth?.State == TargetHealthStateEnum.Healthy),
            targets.Count(target => target.TargetHealth?.State != TargetHealthStateEnum.Draining));
    }

    private async Task<TrafficFacts?> TrafficAsync(AwsClients clients, TaskMetadata metadata, string? targetGroupArn, CancellationToken cancellationToken)
    {
        var targetGroup = AwsArns.TargetGroupDimension(targetGroupArn);
        string? loadBalancer = null;
        if (targetGroup is not null)
        {
            var groups = await clients.LoadBalancing.DescribeTargetGroupsAsync(new DescribeTargetGroupsRequest
            {
                TargetGroupArns = [targetGroupArn!],
            }, cancellationToken);
            loadBalancer = AwsArns.LoadBalancerDimension(groups.TargetGroups?.FirstOrDefault()?.LoadBalancerArns?.FirstOrDefault());
        }

        var queries = new List<MetricDataQuery>();
        if (loadBalancer is not null && targetGroup is not null)
        {
            MetricDimension[] dimensions = [Dimension("LoadBalancer", loadBalancer), Dimension("TargetGroup", targetGroup)];
            queries.Add(Query("requests", "AWS/ApplicationELB", "RequestCount", dimensions, 3600, "Sum"));
            queries.Add(Query("latency", "AWS/ApplicationELB", "TargetResponseTime", dimensions, 3600, "Average"));
        }

        if (metadata.ClusterName is { } cluster && !string.IsNullOrWhiteSpace(metadata.ServiceName))
        {
            MetricDimension[] dimensions = [Dimension("ClusterName", cluster), Dimension("ServiceName", metadata.ServiceName)];
            queries.Add(Query("cpu", "AWS/ECS", "CPUUtilization", dimensions, 300, "Average"));
            queries.Add(Query("memory", "AWS/ECS", "MemoryUtilization", dimensions, 300, "Average"));
        }

        if (queries.Count == 0)
            return null;

        var now = timeProvider.GetUtcNow();
        var currentHour = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero);
        var firstHour = currentHour.AddHours(-(TrafficSummary.Hours - 1));
        var points = queries.ToDictionary(query => query.Id, _ => new List<MetricPoint>());
        string? nextToken = null;
        do
        {
            var page = await clients.CloudWatch.GetMetricDataAsync(new GetMetricDataRequest
            {
                StartTime = firstHour.UtcDateTime,
                EndTime = currentHour.AddHours(1).UtcDateTime,
                MetricDataQueries = queries,
                ScanBy = ScanBy.TimestampAscending,
                NextToken = nextToken,
            }, cancellationToken);
            foreach (var result in page.MetricDataResults ?? [])
            {
                if (result.Id is null || !points.TryGetValue(result.Id, out var series))
                    continue;
                var timestamps = result.Timestamps ?? [];
                var values = result.Values ?? [];
                for (var index = 0; index < Math.Min(timestamps.Count, values.Count); index++)
                    series.Add(new MetricPoint(AsUtc(timestamps[index])!.Value, values[index]));
            }

            nextToken = page.NextToken;
        }
        while (!string.IsNullOrEmpty(nextToken));

        return TrafficSummary.Summarise(
            firstHour,
            now.AddHours(-1),
            points.GetValueOrDefault("requests"),
            points.GetValueOrDefault("latency"),
            points.GetValueOrDefault("cpu"),
            points.GetValueOrDefault("memory"));
    }

    private async Task<ManagedDatabase?> ManagedDatabaseAsync(AwsClients clients, string? instanceId, CancellationToken cancellationToken)
    {
        if (database.Provider != DatabaseProvider.MySql || string.IsNullOrWhiteSpace(instanceId))
            return null;

        var response = await clients.Rds.DescribeDBInstancesAsync(new DescribeDBInstancesRequest
        {
            DBInstanceIdentifier = instanceId,
        }, cancellationToken);
        var instance = response.DBInstances?.FirstOrDefault();
        return instance is null ? null : new ManagedDatabase(EngineName(instance.Engine), instance.EngineVersion, instance.MultiAZ);
    }

    private static async Task<DateTimeOffset?> CertificateValidUntilAsync(AwsClients clients, string? certificateArn, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(certificateArn))
            return null;

        var response = await clients.Certificates.DescribeCertificateAsync(new DescribeCertificateRequest
        {
            CertificateArn = certificateArn,
        }, cancellationToken);
        return AsUtc(response.Certificate?.NotAfter);
    }

    private static MetricDataQuery Query(string id, string metricNamespace, string metricName, MetricDimension[] dimensions, int period, string statistic) => new()
    {
        Id = id,
        ReturnData = true,
        MetricStat = new MetricStat
        {
            Metric = new Metric { Namespace = metricNamespace, MetricName = metricName, Dimensions = [.. dimensions] },
            Period = period,
            Stat = statistic,
        },
    };

    private static MetricDimension Dimension(string name, string value) => new() { Name = name, Value = value };

    internal static string EngineName(string? engine) => engine?.ToLowerInvariant() switch
    {
        "mysql" => "MySQL",
        "mariadb" => "MariaDB",
        "postgres" => "PostgreSQL",
        "aurora-mysql" => "Aurora MySQL",
        "aurora-postgresql" => "Aurora PostgreSQL",
        null or "" => "MySQL",
        _ => engine,
    };

    private static DateTimeOffset? AsUtc(DateTime? value) => value switch
    {
        null => null,
        { Kind: DateTimeKind.Unspecified } unspecified => new DateTimeOffset(DateTime.SpecifyKind(unspecified, DateTimeKind.Utc)),
        { } known => new DateTimeOffset(known.ToUniversalTime()),
    };

    private sealed record DescribedService(ServiceFacts Facts, string? TargetGroupArn);

    private sealed record TargetCounts(int Healthy, int Registered);

    private sealed class AwsClients : IDisposable
    {
        public AwsClients(string region, TimeSpan timeout)
        {
            Region = region;
            var endpoint = RegionEndpoint.GetBySystemName(region);
            Ecs = new AmazonECSClient(Configure(new AmazonECSConfig(), endpoint, timeout));
            LoadBalancing = new AmazonElasticLoadBalancingV2Client(Configure(new AmazonElasticLoadBalancingV2Config(), endpoint, timeout));
            CloudWatch = new AmazonCloudWatchClient(Configure(new AmazonCloudWatchConfig(), endpoint, timeout));
            Rds = new AmazonRDSClient(Configure(new AmazonRDSConfig(), endpoint, timeout));
            Certificates = new AmazonCertificateManagerClient(Configure(new AmazonCertificateManagerConfig(), endpoint, timeout));
        }

        public string Region { get; }

        public AmazonECSClient Ecs { get; }

        public AmazonElasticLoadBalancingV2Client LoadBalancing { get; }

        public AmazonCloudWatchClient CloudWatch { get; }

        public AmazonRDSClient Rds { get; }

        public AmazonCertificateManagerClient Certificates { get; }

        public void Dispose()
        {
            Ecs.Dispose();
            LoadBalancing.Dispose();
            CloudWatch.Dispose();
            Rds.Dispose();
            Certificates.Dispose();
        }

        private static T Configure<T>(T config, RegionEndpoint endpoint, TimeSpan timeout) where T : ClientConfig
        {
            config.RegionEndpoint = endpoint;
            config.Timeout = timeout;
            config.MaxErrorRetry = 1;
            return config;
        }
    }
}
