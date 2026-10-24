using KodingTask.Api.Contracts;
using KodingTask.Api.Limits;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Translations;

public sealed record TermRegistration(int Added, ErrorResponse? Error);

public sealed class TermRegistry(
    KodingTaskDbContext database,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    TranslationIndex index,
    TranslationBatchSignal signal,
    TimeProvider timeProvider)
{
    public const int MaxTexts = 20;
    public const int MaxLength = 300;

    public async Task<TermRegistration> RegisterAsync(IReadOnlyList<string?>? texts, string clientFingerprint, CancellationToken cancellationToken)
    {
        if (texts is null || texts.Count is 0 or > MaxTexts)
            return new TermRegistration(0, ApiErrors.InvalidTerms);
        var cleaned = new List<string>();
        foreach (var text in texts)
        {
            var trimmed = text?.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxLength || trimmed.Any(char.IsControl))
                return new TermRegistration(0, ApiErrors.InvalidTerms);
            if (!cleaned.Contains(trimmed, StringComparer.Ordinal))
                cleaned.Add(trimmed);
        }

        var hashes = cleaned.Select(TermText.Hash).ToList();
        var known = await database.Terms.Where(term => hashes.Contains(term.TextHash)).ToListAsync(cancellationToken);
        foreach (var term in known.Where(term => !term.IsActive && term.Origin == TermOrigin.Visitor))
            term.IsActive = true;
        var fresh = cleaned.Where(text => known.All(term => term.TextHash != TermText.Hash(text))).ToList();

        var limits = await limitsSource.GetAsync(cancellationToken);
        if (fresh.Count > 0 && limits.KillSwitch)
            return new TermRegistration(0, ApiErrors.Paused);

        var added = 0;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var text in fresh)
        {
            if (!await limiter.TryConsumeAsync(Counters.TermRegistrations, limits.TermRegistrationsPerDay, cancellationToken)
                || !await limiter.TryConsumeAsync(Counters.TermRegistrationsByClient(clientFingerprint), limits.TermRegistrationsPerClientPerDay, cancellationToken))
            {
                await SaveAsync(added, cancellationToken);
                return new TermRegistration(added, added == 0 ? ApiErrors.TermLimitReached : null);
            }

            database.Terms.Add(new Term { Text = text, TextHash = TermText.Hash(text), Origin = TermOrigin.Visitor, IsActive = true, CreatedAtUtc = now });
            added++;
        }

        await SaveAsync(added, cancellationToken);
        return new TermRegistration(added, null);
    }

    /// <summary>Stores the texts the server used without knowing them; trusted, so no caps.</summary>
    public async Task<int> AddServerTermsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
            return 0;
        var hashes = texts.Select(TermText.Hash).ToList();
        var known = await database.Terms.Where(term => hashes.Contains(term.TextHash)).Select(term => term.TextHash).ToListAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var added = 0;
        foreach (var text in texts.Where(text => !known.Contains(TermText.Hash(text))))
        {
            database.Terms.Add(new Term { Text = text, TextHash = TermText.Hash(text), Origin = TermOrigin.Server, IsActive = true, CreatedAtUtc = now });
            added++;
        }

        await SaveAsync(added, cancellationToken);
        return added;
    }

    private async Task SaveAsync(int added, CancellationToken cancellationToken)
    {
        try
        {
            await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            database.ChangeTracker.Clear();
        }

        if (added > 0)
        {
            index.Invalidate();
            signal.Notify();
        }
    }
}
