using KodingTask.Api.Ai;
using KodingTask.Api.Limits;
using KodingTask.Conversion;
using KodingTask.Data.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace KodingTask.Api.Tests;

public sealed class AiConversionServiceTests : IDisposable
{
    private static readonly LanguageInfo English = SpellerCatalog.Default.Languages[0];
    private static readonly MonetaryAmount Amount = new(25, 10);
    private const string Reference = "twenty-five dollars and ten cents";

    private readonly TestDatabase _database = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeModel _model = new();
    private readonly AiOptions _aiOptions = new() { ModelId = "test-model", Region = "eu-central-1", ModelLabel = "Test model" };
    private LimitsOptions _limits = new() { AiCallsPerDay = 10, AiCallsPerClientPerDay = 5, AiMonthlyBudgetUsd = 1m };

    [Fact]
    public async Task Reports_agreement_and_logs_the_call_with_its_cost()
    {
        _model.Reply = new AiModelReply("\"Twenty-five dollars and ten cents.\"", "end_turn", 1_000, 200, 0, 0);

        var outcome = await Service().ConvertAsync(English, Amount, Reference, "client", CancellationToken.None);

        Assert.Equal("Twenty-five dollars and ten cents", outcome.Words);
        Assert.True(outcome.Agrees);
        Assert.Null(outcome.Refusal);
        var call = Assert.Single(_database.CreateContext().AiCalls);
        Assert.Equal(AiCallOutcome.Agreed, call.Outcome);
        Assert.Equal(200, call.CostMicroUsd);
        Assert.Equal("client", call.ClientHash);
    }

    [Fact]
    public async Task Reports_a_disagreement()
    {
        _model.Reply = new AiModelReply("twenty-five dollars and one cent", "end_turn", 10, 10, 0, 0);

        var outcome = await Service().ConvertAsync(English, Amount, Reference, "client", CancellationToken.None);

        Assert.False(outcome.Agrees);
        Assert.Equal(AiCallOutcome.Differed, Assert.Single(_database.CreateContext().AiCalls).Outcome);
    }

    [Theory]
    [InlineData("Sure! Here it is:\ntwenty-five dollars", "end_turn")]
    [InlineData("25 dollars and 10 cents", "end_turn")]
    [InlineData("twenty-five dollars and", "max_tokens")]
    [InlineData("", "refusal")]
    public async Task Rejects_unexpected_output_but_still_logs_it(string text, string stopReason)
    {
        _model.Reply = new AiModelReply(text, stopReason, 10, 10, 0, 0);

        var outcome = await Service().ConvertAsync(English, Amount, Reference, "client", CancellationToken.None);

        Assert.Equal(AiRefusal.Failed, outcome.Refusal);
        Assert.Equal(AiCallOutcome.UnexpectedOutput, Assert.Single(_database.CreateContext().AiCalls).Outcome);
    }

    [Fact]
    public async Task Logs_a_failed_model_call()
    {
        _model.Failure = new TimeoutException();

        var outcome = await Service().ConvertAsync(English, Amount, Reference, "client", CancellationToken.None);

        Assert.Equal(AiRefusal.Failed, outcome.Refusal);
        var call = Assert.Single(_database.CreateContext().AiCalls);
        Assert.Equal(AiCallOutcome.Failed, call.Outcome);
        Assert.Equal(nameof(TimeoutException), call.Error);
    }

    [Fact]
    public async Task Refuses_when_not_configured_without_calling_the_model()
    {
        _aiOptions.ModelId = null;

        var outcome = await Service().ConvertAsync(English, Amount, Reference, "client", CancellationToken.None);

        Assert.Equal(AiRefusal.Unavailable, outcome.Refusal);
        Assert.Equal(0, _model.Calls);
    }

    [Fact]
    public async Task Refuses_when_the_kill_switch_is_on()
    {
        _limits = new LimitsOptions { KillSwitch = true, AiCallsPerDay = 10, AiCallsPerClientPerDay = 10, AiMonthlyBudgetUsd = 1m };

        var outcome = await Service().ConvertAsync(English, Amount, Reference, "client", CancellationToken.None);

        Assert.Equal(AiRefusal.Paused, outcome.Refusal);
        Assert.Equal(0, _model.Calls);
    }

    [Fact]
    public async Task Enforces_the_daily_and_per_client_limits()
    {
        _limits = new LimitsOptions { AiCallsPerDay = 3, AiCallsPerClientPerDay = 2, AiMonthlyBudgetUsd = 1m };
        _model.Reply = new AiModelReply(Reference, "end_turn", 10, 10, 0, 0);

        var service = Service();
        var a1 = await service.ConvertAsync(English, Amount, Reference, "a", CancellationToken.None);
        var a2 = await service.ConvertAsync(English, Amount, Reference, "a", CancellationToken.None);
        var a3 = await service.ConvertAsync(English, Amount, Reference, "a", CancellationToken.None);
        var b1 = await service.ConvertAsync(English, Amount, Reference, "b", CancellationToken.None);
        var c1 = await service.ConvertAsync(English, Amount, Reference, "c", CancellationToken.None);

        Assert.Null(a1.Refusal);
        Assert.Null(a2.Refusal);
        Assert.Equal(AiRefusal.ClientLimitReached, a3.Refusal);
        Assert.Equal(AiRefusal.DailyLimitReached, b1.Refusal);
        Assert.Equal(AiRefusal.DailyLimitReached, c1.Refusal);
        Assert.Equal(2, _model.Calls);
    }

    [Fact]
    public async Task Stops_when_the_monthly_budget_is_spent()
    {
        _limits = new LimitsOptions { AiCallsPerDay = 100, AiCallsPerClientPerDay = 100, AiMonthlyBudgetUsd = 0.0005m };
        _model.Reply = new AiModelReply(Reference, "end_turn", 1_000, 1_000, 0, 0);

        var service = Service();
        var first = await service.ConvertAsync(English, Amount, Reference, "a", CancellationToken.None);
        var second = await service.ConvertAsync(English, Amount, Reference, "a", CancellationToken.None);

        Assert.Null(first.Refusal);
        Assert.Equal(AiRefusal.MonthlyBudgetReached, second.Refusal);
        Assert.Equal(1, _model.Calls);
    }

    [Fact]
    public void Prompt_teaches_every_language_and_the_regional_french_numbers()
    {
        var prompt = new AiPromptBuilder(SpellerCatalog.Default).SystemPrompt;

        foreach (var language in SpellerCatalog.Default.Languages)
            Assert.Contains($"{language.Code} | 999 999 999,99 | ", prompt);
        Assert.Contains("fr-CH | 80 | huitante dollars", prompt);
        Assert.Contains("fr-BE | 91 | nonante et un dollars", prompt);
        Assert.Contains("zh-CN | 2 | 两美元", prompt);
        Assert.Contains("ko-KR | 11 000 | 만 천 달러", prompt);
        Assert.Contains("fr-FR | 71 | soixante et onze dollars", prompt);
    }

    private AiConversionService Service()
    {
        var database = _database.CreateContext();
        var limitsSource = new UsageLimitsSource(Options.Create(_limits), _clock, NullLogger<UsageLimitsSource>.Instance);
        return new AiConversionService(
            _model,
            new AiPromptBuilder(SpellerCatalog.Default),
            new AiConcurrencyGate(Options.Create(_aiOptions)),
            limitsSource,
            new UsageLimiter(database, _clock),
            database,
            Options.Create(_aiOptions),
            _clock,
            NullLogger<AiConversionService>.Instance);
    }

    public void Dispose() => _database.Dispose();

    private sealed class FakeModel : IAiModelClient
    {
        public AiModelReply Reply { get; set; } = new("", "end_turn", 0, 0, 0, 0);

        public Exception? Failure { get; set; }

        public int Calls { get; private set; }

        public Task<AiModelReply> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
        {
            Calls++;
            return Failure is null ? Task.FromResult(Reply) : Task.FromException<AiModelReply>(Failure);
        }
    }
}
