namespace KodingTask.Api.Limits;

public sealed record UsageLimits
{
    public bool KillSwitch { get; init; }

    public int AiCallsPerDay { get; init; }

    public int AiCallsPerClientPerDay { get; init; }

    public decimal AiMonthlyBudgetUsd { get; init; }

    public static UsageLimits Closed { get; } = new() { KillSwitch = true };
}
