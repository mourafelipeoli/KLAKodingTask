namespace KodingTask.Api.Limits;

public sealed class LimitsOptions
{
    public const string SectionName = "Limits";

    public string? SsmPath { get; set; }

    public string? SsmRegion { get; set; }

    public int CacheSeconds { get; set; } = 60;

    public bool KillSwitch { get; set; }

    public int AiCallsPerDay { get; set; } = 200;

    public int AiCallsPerClientPerDay { get; set; } = 20;

    public decimal AiMonthlyBudgetUsd { get; set; } = 2m;

    public int EmailsPerDay { get; set; } = 100;

    public int WhatsAppPerDay { get; set; } = 50;

    public int DeliveriesPerUserPerDay { get; set; } = 10;

    public int TranslationCharactersPerDay { get; set; } = 50_000;

    public int TermRegistrationsPerDay { get; set; } = 200;

    public int TermRegistrationsPerClientPerDay { get; set; } = 20;
}
