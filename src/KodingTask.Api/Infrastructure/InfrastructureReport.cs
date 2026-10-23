using KodingTask.Api.Features;

namespace KodingTask.Api.Infrastructure;

public sealed record InfrastructureReport(
    DateTimeOffset CheckedAt,
    HostingFacts Hosting,
    ServiceFacts? Service,
    TrafficFacts? Traffic,
    BuildFacts Build,
    DataFacts Data,
    SecurityFacts Security,
    GuardrailFacts Guardrails,
    FeatureAvailability Features);

public sealed record HostingFacts(
    bool OnAws,
    string? Region,
    string? RegionName,
    string? AvailabilityZone,
    string? Compute,
    double? Vcpu,
    int? MemoryMiB,
    DateTimeOffset? StartedAt,
    string Runtime,
    string OperatingSystem,
    string Architecture);

public sealed record ServiceFacts(
    int Running,
    int Desired,
    int? HealthyTargets,
    int? RegisteredTargets,
    string? RolloutState,
    DateTimeOffset? LastDeployedAt,
    bool AutomaticRollback,
    bool PublicIpAddress);

public sealed record TrafficFacts(
    long? Requests24h,
    IReadOnlyList<long>? RequestsPerHour,
    double? AverageResponseMs24h,
    double? CpuPercent1h,
    double? MemoryPercent1h);

public sealed record BuildFacts(string? Version, string? Commit, string? CommitUrl, string? BuiltAt);

public sealed record DataFacts(string Engine, string? Version, string? ManagedBy, bool Persistent, bool? MultiAz);

public sealed record SecurityFacts(bool Https, DateTimeOffset? CertificateValidUntil, bool LimitsOutsideCode);

public sealed record GuardrailFacts(
    bool KillSwitch,
    int AiCallsToday,
    int AiCallsPerDay,
    decimal AiSpendThisMonthUsd,
    decimal AiMonthlyBudgetUsd,
    int EmailsToday,
    int EmailsPerDay,
    int WhatsAppToday,
    int WhatsAppPerDay);
