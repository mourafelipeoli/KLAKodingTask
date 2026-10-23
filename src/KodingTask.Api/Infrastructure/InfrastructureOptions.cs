namespace KodingTask.Api.Infrastructure;

public sealed class InfrastructureOptions
{
    public const string SectionName = "Infrastructure";

    public string? DatabaseInstanceId { get; set; }

    public string? CertificateArn { get; set; }

    public int CacheMinutes { get; set; } = 5;

    public int AwsCallTimeoutSeconds { get; set; } = 5;
}
