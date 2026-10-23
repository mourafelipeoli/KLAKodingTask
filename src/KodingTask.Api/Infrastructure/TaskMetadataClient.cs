using System.Text.Json;
using System.Text.Json.Serialization;

namespace KodingTask.Api.Infrastructure;

public interface ITaskMetadataSource
{
    Task<TaskMetadata?> ReadAsync(CancellationToken cancellationToken);
}

public sealed class TaskMetadataClient(HttpClient http, IConfiguration configuration, ILogger<TaskMetadataClient> logger) : ITaskMetadataSource
{
    public const string EndpointVariable = "ECS_CONTAINER_METADATA_URI_V4";

    public async Task<TaskMetadata?> ReadAsync(CancellationToken cancellationToken)
    {
        var endpoint = configuration[EndpointVariable];
        if (string.IsNullOrWhiteSpace(endpoint))
            return null;

        try
        {
            return await http.GetFromJsonAsync<TaskMetadata>($"{endpoint.TrimEnd('/')}/task", cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogWarning("[Infrastructure] Task metadata unavailable: {Error}.", exception.GetType().Name);
            return null;
        }
    }
}

public sealed record TaskMetadata(
    [property: JsonPropertyName("Cluster")] string? Cluster,
    [property: JsonPropertyName("ServiceName")] string? ServiceName,
    [property: JsonPropertyName("TaskARN")] string? TaskArn,
    [property: JsonPropertyName("AvailabilityZone")] string? AvailabilityZone,
    [property: JsonPropertyName("LaunchType")] string? LaunchType,
    [property: JsonPropertyName("Limits")] TaskLimits? Limits,
    [property: JsonPropertyName("PullStoppedAt")] DateTimeOffset? PullStoppedAt,
    [property: JsonPropertyName("Containers")] IReadOnlyList<ContainerMetadata>? Containers)
{
    public string? Region => AwsArns.Region(TaskArn);

    public string? ClusterName => AwsArns.ClusterName(Cluster);

    public DateTimeOffset? StartedAt =>
        Containers?
            .Where(container => container.Type is null or "NORMAL")
            .Select(container => container.StartedAt)
            .Where(started => started is not null)
            .Min()
        ?? PullStoppedAt;
}

public sealed record TaskLimits(
    [property: JsonPropertyName("CPU")] double? Cpu,
    [property: JsonPropertyName("Memory")] int? Memory);

public sealed record ContainerMetadata(
    [property: JsonPropertyName("Name")] string? Name,
    [property: JsonPropertyName("Type")] string? Type,
    [property: JsonPropertyName("StartedAt")] DateTimeOffset? StartedAt);
