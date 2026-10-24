namespace KodingTask.Api.Translations;

public sealed class DeepLOptions
{
    public const string SectionName = "DeepL";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Optional; derived from the key otherwise (free keys end in ":fx").</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public int BatchSize { get; set; } = 50;

    public int IntervalSeconds { get; set; } = 120;

    public int TimeoutSeconds { get; set; } = 20;

    public double[]? RetryDelaysSeconds { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public string EffectiveBaseUrl =>
        !string.IsNullOrWhiteSpace(BaseUrl) ? BaseUrl.TrimEnd('/')
        : ApiKey.EndsWith(":fx", StringComparison.Ordinal) ? "https://api-free.deepl.com"
        : "https://api.deepl.com";

    public IReadOnlyList<TimeSpan> RetryDelays =>
        (RetryDelaysSeconds is { Length: > 0 } seconds ? seconds : [1, 3, 8, 20]).Select(TimeSpan.FromSeconds).ToArray();
}
