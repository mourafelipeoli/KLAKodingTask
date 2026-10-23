using KodingTask.Api.Limits;
using KodingTask.Conversion;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Ai;

public sealed class AiConversionService(
    IAiModelClient model,
    AiPromptBuilder prompts,
    AiConcurrencyGate gate,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    KodingTaskDbContext database,
    IOptions<AiOptions> options,
    TimeProvider timeProvider,
    ILogger<AiConversionService> logger)
{
    public const string DailyCounter = "ai:calls";

    public static string ClientCounter(string clientHash) => $"ai:client:{clientHash}";

    public async Task<AiConversionOutcome> ConvertAsync(
        LanguageInfo language,
        MonetaryAmount amount,
        string referenceWords,
        string clientHash,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
            return AiConversionOutcome.Refused(AiRefusal.Unavailable);

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return AiConversionOutcome.Refused(AiRefusal.Paused);
        if (await MonthToDateMicroUsdAsync(cancellationToken) >= ToMicroUsd(limits.AiMonthlyBudgetUsd))
            return AiConversionOutcome.Refused(AiRefusal.MonthlyBudgetReached);
        if (!await limiter.TryConsumeAsync(DailyCounter, limits.AiCallsPerDay, cancellationToken))
            return AiConversionOutcome.Refused(AiRefusal.DailyLimitReached);
        if (!await limiter.TryConsumeAsync(ClientCounter(clientHash), limits.AiCallsPerClientPerDay, cancellationToken))
            return AiConversionOutcome.Refused(AiRefusal.ClientLimitReached);
        if (!await gate.TryEnterAsync(cancellationToken))
            return AiConversionOutcome.Refused(AiRefusal.Busy);

        try
        {
            return await CallModelAsync(language, amount, referenceWords, clientHash, settings, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<AiConversionOutcome> CallModelAsync(
        LanguageInfo language,
        MonetaryAmount amount,
        string referenceWords,
        string clientHash,
        AiOptions settings,
        CancellationToken cancellationToken)
    {
        var started = timeProvider.GetTimestamp();
        AiModelReply? reply = null;
        string? error = null;
        try
        {
            reply = await model.CompleteAsync(prompts.SystemPrompt, prompts.UserPrompt(language, amount), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            error = exception.GetType().Name;
            logger.LogWarning("[Ai] The model call failed: {Error}.", error);
        }

        var latencyMs = (int)timeProvider.GetElapsedTime(started).TotalMilliseconds;
        var words = reply?.StopReason is "end_turn" or null ? AiAnswer.Clean(reply?.Text) : null;
        var outcome = (reply, words) switch
        {
            (null, _) => AiCallOutcome.Failed,
            (_, null) => AiCallOutcome.UnexpectedOutput,
            _ when AiAnswer.SameWords(words, referenceWords) => AiCallOutcome.Agreed,
            _ => AiCallOutcome.Differed,
        };
        if (outcome == AiCallOutcome.UnexpectedOutput)
            error = $"stop reason {reply?.StopReason ?? "unknown"}";

        database.AiCalls.Add(new AiCall
        {
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
            Model = settings.ModelLabel,
            Language = language.Code,
            Dollars = amount.Dollars,
            Cents = amount.Cents,
            Outcome = outcome,
            InputTokens = reply?.InputTokens ?? 0,
            OutputTokens = reply?.OutputTokens ?? 0,
            CacheReadTokens = reply?.CacheReadTokens ?? 0,
            CacheWriteTokens = reply?.CacheWriteTokens ?? 0,
            CostMicroUsd = reply is null ? 0 : CostMicroUsd(reply, settings.Pricing),
            LatencyMs = latencyMs,
            ClientHash = clientHash,
            Error = error,
        });
        await database.SaveChangesAsync(CancellationToken.None);

        logger.LogInformation("[Ai] {Language} answered in {LatencyMs} ms: {Outcome}.", language.Code, latencyMs, outcome);
        return words is null
            ? AiConversionOutcome.Refused(AiRefusal.Failed)
            : AiConversionOutcome.Answered(words, outcome == AiCallOutcome.Agreed, latencyMs);
    }

    private async Task<long> MonthToDateMicroUsdAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return await database.AiCalls
            .Where(call => call.CreatedAtUtc >= monthStart)
            .SumAsync(call => (long?)call.CostMicroUsd, cancellationToken) ?? 0;
    }

    internal static long CostMicroUsd(AiModelReply reply, AiPricing pricing) => (long)Math.Ceiling(
        reply.InputTokens * pricing.InputPerMillionUsd
        + reply.OutputTokens * pricing.OutputPerMillionUsd
        + reply.CacheReadTokens * pricing.CacheReadPerMillionUsd
        + reply.CacheWriteTokens * pricing.CacheWritePerMillionUsd);

    private static long ToMicroUsd(decimal usd) => (long)Math.Floor(usd * 1_000_000m);
}
