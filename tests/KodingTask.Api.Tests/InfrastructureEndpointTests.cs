using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KodingTask.Api.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KodingTask.Api.Tests;

public sealed class InfrastructureEndpointTests
{
    private const string Account = "123456789012";

    private static readonly TaskMetadata OnFargate = new(
        Cluster: $"arn:aws:ecs:us-east-2:{Account}:cluster/private-cluster",
        ServiceName: "private-service",
        TaskArn: $"arn:aws:ecs:us-east-2:{Account}:task/private-cluster/0123456789abcdef",
        AvailabilityZone: "us-east-2b",
        LaunchType: "FARGATE",
        Limits: new TaskLimits(0.25, 512),
        PullStoppedAt: new DateTimeOffset(2026, 10, 23, 9, 59, 0, TimeSpan.Zero),
        Containers: [new ContainerMetadata("app", "NORMAL", new DateTimeOffset(2026, 10, 23, 10, 0, 0, TimeSpan.Zero))]);

    [Fact]
    public async Task Running_locally_reports_no_aws_facts_and_the_local_database()
    {
        using var factory = new ApiFactory();
        var report = await factory.CreateClient().GetFromJsonAsync<JsonElement>("/api/infrastructure");

        var hosting = report.GetProperty("hosting");
        Assert.False(hosting.GetProperty("onAws").GetBoolean());
        Assert.Equal(JsonValueKind.Null, hosting.GetProperty("region").ValueKind);
        Assert.Contains(".NET", hosting.GetProperty("runtime").GetString());
        Assert.Equal(JsonValueKind.Null, report.GetProperty("service").ValueKind);
        Assert.Equal(JsonValueKind.Null, report.GetProperty("traffic").ValueKind);

        var data = report.GetProperty("data");
        Assert.Equal("SQLite", data.GetProperty("engine").GetString());
        Assert.True(data.GetProperty("persistent").GetBoolean());

        var guardrails = report.GetProperty("guardrails");
        Assert.False(guardrails.GetProperty("killSwitch").GetBoolean());
        Assert.Equal(0, guardrails.GetProperty("aiCallsToday").GetInt32());
        Assert.Equal(200, guardrails.GetProperty("aiCallsPerDay").GetInt32());
        Assert.Equal(2m, guardrails.GetProperty("aiMonthlyBudgetUsd").GetDecimal());
        Assert.False(report.GetProperty("security").GetProperty("limitsOutsideCode").GetBoolean());
        Assert.False(report.GetProperty("features").GetProperty("ai").GetBoolean());
    }

    [Fact]
    public async Task On_aws_reports_facts_in_words_and_never_resource_names()
    {
        var probe = new FakeProbe(new AwsSnapshot(
            new ServiceFacts(1, 1, 1, 1, "COMPLETED", DateTimeOffset.UtcNow.AddHours(-2), true, false),
            new TrafficFacts(42, new long[24], 18.5, 3.2, 21.0),
            new DateTimeOffset(2027, 11, 20, 23, 59, 59, TimeSpan.Zero),
            null));
        using var factory = OnAws(probe);

        var body = await factory.CreateClient().GetStringAsync("/api/infrastructure");

        Assert.DoesNotContain("arn:", body);
        Assert.DoesNotContain(Account, body);
        Assert.DoesNotContain("private-cluster", body);
        Assert.DoesNotContain("private-service", body);
        Assert.DoesNotContain("0123456789abcdef", body);

        var report = JsonDocument.Parse(body).RootElement;
        var hosting = report.GetProperty("hosting");
        Assert.True(hosting.GetProperty("onAws").GetBoolean());
        Assert.Equal("US East (Ohio)", hosting.GetProperty("regionName").GetString());
        Assert.Equal("AWS Fargate", hosting.GetProperty("compute").GetString());
        Assert.Equal(0.25, hosting.GetProperty("vcpu").GetDouble());
        Assert.Equal(512, hosting.GetProperty("memoryMiB").GetInt32());
        Assert.Equal("COMPLETED", report.GetProperty("service").GetProperty("rolloutState").GetString());
        Assert.Equal(42, report.GetProperty("traffic").GetProperty("requests24h").GetInt64());
        Assert.False(report.GetProperty("data").GetProperty("persistent").GetBoolean());
    }

    [Fact]
    public async Task A_failing_probe_still_answers_with_the_hosting_facts()
    {
        using var factory = OnAws(new FakeProbe(failure: new InvalidOperationException("boom")));

        var response = await factory.CreateClient().GetAsync("/api/infrastructure");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(report.GetProperty("hosting").GetProperty("onAws").GetBoolean());
        Assert.Equal(JsonValueKind.Null, report.GetProperty("service").ValueKind);
    }

    [Fact]
    public async Task Asks_aws_once_for_many_visits()
    {
        var probe = new FakeProbe(AwsSnapshot.Empty);
        using var factory = OnAws(probe);
        var client = factory.CreateClient();

        for (var visit = 0; visit < 3; visit++)
            (await client.GetAsync("/api/infrastructure")).EnsureSuccessStatusCode();

        Assert.Equal(1, probe.Calls);
    }

    [Fact]
    public async Task Builds_the_commit_link_only_with_a_public_repository()
    {
        using var factory = new ApiFactory();
        factory.Settings["BUILD_COMMIT"] = "abcdef123456";
        factory.Settings["PublicSite:RepositoryUrl"] = "https://example.com/someone/repo/";

        var build = (await factory.CreateClient().GetFromJsonAsync<JsonElement>("/api/infrastructure")).GetProperty("build");

        Assert.Equal("https://example.com/someone/repo/commit/abcdef123456", build.GetProperty("commitUrl").GetString());
    }

    private static ApiFactory OnAws(IAwsInfrastructureProbe probe)
    {
        var factory = new ApiFactory();
        factory.ServiceOverrides.Add(services =>
        {
            services.RemoveAll<ITaskMetadataSource>();
            services.AddSingleton<ITaskMetadataSource>(new FakeMetadata(OnFargate));
            services.RemoveAll<IAwsInfrastructureProbe>();
            services.AddSingleton(probe);
        });
        return factory;
    }

    private sealed class FakeMetadata(TaskMetadata metadata) : ITaskMetadataSource
    {
        public Task<TaskMetadata?> ReadAsync(CancellationToken cancellationToken) => Task.FromResult<TaskMetadata?>(metadata);
    }

    private sealed class FakeProbe(AwsSnapshot? snapshot = null, Exception? failure = null) : IAwsInfrastructureProbe
    {
        public int Calls { get; private set; }

        public Task<AwsSnapshot> ProbeAsync(TaskMetadata metadata, CancellationToken cancellationToken)
        {
            Calls++;
            return failure is null ? Task.FromResult(snapshot ?? AwsSnapshot.Empty) : Task.FromException<AwsSnapshot>(failure);
        }
    }
}
