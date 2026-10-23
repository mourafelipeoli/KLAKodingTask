using System.Text.Json;
using KodingTask.Api.Infrastructure;

namespace KodingTask.Api.Tests;

public sealed class InfrastructureFactsTests
{
    private static readonly DateTimeOffset FirstHour = new(2026, 10, 22, 11, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("arn:aws:ecs:us-east-2:123456789012:task/cluster/abc", "us-east-2")]
    [InlineData("arn:aws:ecs:eu-central-1:123456789012:task/abc", "eu-central-1")]
    [InlineData("not-an-arn", null)]
    [InlineData(null, null)]
    public void Reads_the_region_from_an_arn(string? arn, string? region) =>
        Assert.Equal(region, AwsArns.Region(arn));

    [Theory]
    [InlineData("arn:aws:ecs:us-east-2:123456789012:cluster/apps", "apps")]
    [InlineData("apps", "apps")]
    [InlineData("", null)]
    public void Reads_the_cluster_name_from_an_arn_or_a_short_name(string cluster, string? name) =>
        Assert.Equal(name, AwsArns.ClusterName(cluster));

    [Fact]
    public void Builds_the_cloudwatch_dimensions_of_a_load_balancer_and_a_target_group()
    {
        Assert.Equal("targetgroup/tg-app/0123abcd", AwsArns.TargetGroupDimension(
            "arn:aws:elasticloadbalancing:us-east-2:123456789012:targetgroup/tg-app/0123abcd"));
        Assert.Equal("app/alb-shared/4567ef", AwsArns.LoadBalancerDimension(
            "arn:aws:elasticloadbalancing:us-east-2:123456789012:loadbalancer/app/alb-shared/4567ef"));
        Assert.Null(AwsArns.LoadBalancerDimension(null));
    }

    [Fact]
    public void Parses_the_task_metadata_and_ignores_internal_containers()
    {
        const string json = """
            {
              "Cluster": "arn:aws:ecs:us-east-2:123456789012:cluster/apps",
              "ServiceName": "web",
              "TaskARN": "arn:aws:ecs:us-east-2:123456789012:task/apps/abc",
              "AvailabilityZone": "us-east-2a",
              "LaunchType": "FARGATE",
              "Limits": { "CPU": 0.25, "Memory": 512 },
              "PullStoppedAt": "2026-10-23T09:59:00.000Z",
              "Containers": [
                { "Name": "~internal~ecs~pause", "Type": "CNI_PAUSE", "StartedAt": "2026-10-23T09:58:00Z" },
                { "Name": "app", "Type": "NORMAL", "StartedAt": "2026-10-23T10:00:05.123Z" }
              ]
            }
            """;

        var metadata = JsonSerializer.Deserialize<TaskMetadata>(json)!;

        Assert.Equal("us-east-2", metadata.Region);
        Assert.Equal("apps", metadata.ClusterName);
        Assert.Equal("web", metadata.ServiceName);
        Assert.Equal(0.25, metadata.Limits?.Cpu);
        Assert.Equal(512, metadata.Limits?.Memory);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, 10, 0, 5, 123, TimeSpan.Zero), metadata.StartedAt);
    }

    [Fact]
    public void Hosting_facts_name_the_region_and_the_compute_in_words()
    {
        var metadata = new TaskMetadata(null, null, "arn:aws:ecs:us-east-2:123456789012:task/x", "us-east-2c", "FARGATE",
            new TaskLimits(0.5, 1024), null, null);

        var hosting = InfrastructureSnapshotSource.Hosting(metadata);

        Assert.True(hosting.OnAws);
        Assert.Equal("US East (Ohio)", hosting.RegionName);
        Assert.Equal("AWS Fargate", hosting.Compute);
        Assert.False(InfrastructureSnapshotSource.Hosting(null).OnAws);
    }

    [Fact]
    public void Places_requests_in_hourly_buckets_and_weights_the_response_time_by_them()
    {
        MetricPoint[] requests = [new(FirstHour, 10), new(FirstHour.AddHours(5), 30), new(FirstHour.AddHours(23), 2)];
        MetricPoint[] latency = [new(FirstHour, 0.010), new(FirstHour.AddHours(5), 0.050), new(FirstHour.AddHours(7), 9.0)];

        var traffic = TrafficSummary.Summarise(FirstHour, FirstHour.AddHours(23), requests, latency, null, null)!;

        Assert.Equal(42, traffic.Requests24h);
        Assert.Equal(24, traffic.RequestsPerHour!.Count);
        Assert.Equal(10, traffic.RequestsPerHour[0]);
        Assert.Equal(30, traffic.RequestsPerHour[5]);
        Assert.Equal(2, traffic.RequestsPerHour[23]);
        Assert.Equal(40.0, traffic.AverageResponseMs24h);
        Assert.Null(traffic.CpuPercent1h);
    }

    [Fact]
    public void A_quiet_day_is_zero_requests_but_an_unmeasured_day_is_unknown()
    {
        var quiet = TrafficSummary.Summarise(FirstHour, FirstHour, [], [], null, null)!;
        Assert.Equal(0, quiet.Requests24h);
        Assert.Null(quiet.AverageResponseMs24h);

        Assert.Null(TrafficSummary.Summarise(FirstHour, FirstHour, null, null, null, null));
    }

    [Fact]
    public void Averages_cpu_and_memory_over_the_last_hour_only()
    {
        var since = FirstHour.AddHours(23);
        MetricPoint[] cpu = [new(since.AddMinutes(-30), 90), new(since.AddMinutes(10), 2), new(since.AddMinutes(40), 4)];
        MetricPoint[] memory = [new(since.AddMinutes(5), 20.04)];

        var traffic = TrafficSummary.Summarise(FirstHour, since, null, null, cpu, memory)!;

        Assert.Null(traffic.Requests24h);
        Assert.Equal(3.0, traffic.CpuPercent1h);
        Assert.Equal(20.0, traffic.MemoryPercent1h);
    }

    [Theory]
    [InlineData("mysql", "MySQL")]
    [InlineData("aurora-postgresql", "Aurora PostgreSQL")]
    [InlineData("oracle-ee", "oracle-ee")]
    public void Names_database_engines_as_people_know_them(string engine, string name) =>
        Assert.Equal(name, AwsInfrastructureProbe.EngineName(engine));
}
