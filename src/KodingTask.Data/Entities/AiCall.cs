namespace KodingTask.Data.Entities;

public class AiCall
{
    public long Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public required string Model { get; set; }

    public required string Language { get; set; }

    public int Dollars { get; set; }

    public int Cents { get; set; }

    public AiCallOutcome Outcome { get; set; }

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int CacheReadTokens { get; set; }

    public int CacheWriteTokens { get; set; }

    public long CostMicroUsd { get; set; }

    public int LatencyMs { get; set; }

    public required string ClientHash { get; set; }

    public string? Error { get; set; }
}

public enum AiCallOutcome
{
    Agreed,
    Differed,
    UnexpectedOutput,
    Failed,
}
