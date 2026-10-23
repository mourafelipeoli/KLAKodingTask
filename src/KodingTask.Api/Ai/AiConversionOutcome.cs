namespace KodingTask.Api.Ai;

public enum AiRefusal
{
    Unavailable,
    Paused,
    MonthlyBudgetReached,
    DailyLimitReached,
    ClientLimitReached,
    Busy,
    Failed,
}

public sealed record AiConversionOutcome(string? Words, bool Agrees, int LatencyMs, AiRefusal? Refusal)
{
    public static AiConversionOutcome Answered(string words, bool agrees, int latencyMs) => new(words, agrees, latencyMs, null);

    public static AiConversionOutcome Refused(AiRefusal refusal) => new(null, false, 0, refusal);

    public string? ErrorCode => Refusal is { } refusal ? $"Ai{refusal}" : null;
}
