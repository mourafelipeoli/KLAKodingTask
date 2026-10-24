namespace KodingTask.Api.Limits;

public sealed record UsageLimits
{
    public bool KillSwitch { get; init; }

    public int AiCallsPerDay { get; init; }

    public int AiCallsPerClientPerDay { get; init; }

    public decimal AiMonthlyBudgetUsd { get; init; }

    public int EmailsPerDay { get; init; }

    public int WhatsAppPerDay { get; init; }

    public int DeliveriesPerUserPerDay { get; init; }

    public int PhoneCodesPerUserPerDay { get; init; }



    public static UsageLimits Closed { get; } = new() { KillSwitch = true };
}
