namespace KodingTask.Api.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string? ModelId { get; set; }

    public string? Region { get; set; }

    public string ModelLabel { get; set; } = "Claude on Amazon Bedrock";

    public int MaxOutputTokens { get; set; } = 2000;

    public string? Effort { get; set; }

    public int MaxConcurrency { get; set; } = 2;

    public int TimeoutSeconds { get; set; } = 30;

    public AiPricing Pricing { get; set; } = new();

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ModelId) && !string.IsNullOrWhiteSpace(Region);
}

public sealed class AiPricing
{
    public decimal InputPerMillionUsd { get; set; } = 0.10m;

    public decimal OutputPerMillionUsd { get; set; } = 0.50m;

    public decimal CacheReadPerMillionUsd { get; set; } = 0.01m;

    public decimal CacheWritePerMillionUsd { get; set; } = 0.125m;
}
