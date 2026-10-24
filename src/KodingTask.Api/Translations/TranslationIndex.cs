using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using KodingTask.Data;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Translations;

public sealed record LocaleDictionary(string Locale, string Version, IReadOnlyDictionary<string, string> Entries, DateTimeOffset LoadedAt);

public sealed class TranslationIndex(IServiceScopeFactory scopes, TimeProvider timeProvider)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, LocaleDictionary> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _refreshing = new(StringComparer.OrdinalIgnoreCase);

    public async Task<LocaleDictionary> GetAsync(string locale, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(locale, out var cached) && timeProvider.GetUtcNow() - cached.LoadedAt < Lifetime)
            return cached;

        var loaded = await LoadAsync(locale, cancellationToken);
        _cache[locale] = loaded;
        return loaded;
    }

    public void Invalidate() => _cache.Clear();

    /// <summary>The dictionary as loaded, without waiting: a missing or old one is reloaded in the background.</summary>
    public LocaleDictionary? Cached(string locale)
    {
        _cache.TryGetValue(locale, out var cached);
        if (cached is null || timeProvider.GetUtcNow() - cached.LoadedAt >= Lifetime)
            _ = RefreshAsync(locale);
        return cached;
    }

    /// <summary>Whether the text is a term already (the English dictionary lists every active term).</summary>
    public bool KnowsTerm(string text) => Cached(TranslationLocales.Source)?.Entries.ContainsKey(text) ?? true;

    private async Task RefreshAsync(string locale)
    {
        if (!_refreshing.TryAdd(locale, 0))
            return;
        try
        {
            _cache[locale] = await LoadAsync(locale, CancellationToken.None);
        }
        catch (Exception)
        {
            // The next call tries again; until then the bundled dictionaries answer.
        }
        finally
        {
            _refreshing.TryRemove(locale, out _);
        }
    }

    private async Task<LocaleDictionary> LoadAsync(string locale, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
        var terms = await database.Terms.AsNoTracking().Where(term => term.IsActive)
            .Select(term => new { term.Id, term.Text }).ToDictionaryAsync(term => term.Id, term => term.Text, cancellationToken);

        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.Equals(locale, TranslationLocales.Source, StringComparison.OrdinalIgnoreCase))
        {
            foreach (var text in terms.Values)
                entries[text] = text;
        }
        else
        {
            var parent = TranslationLocales.Derived.TryGetValue(locale, out var derivedFrom) ? derivedFrom : locale;
            var rows = await database.Translations.AsNoTracking()
                .Where(translation => translation.Locale == parent || translation.Locale == locale)
                .Select(translation => new { translation.TermId, translation.Locale, translation.Text })
                .ToListAsync(cancellationToken);
            foreach (var row in rows.OrderBy(row => row.Locale == parent ? 0 : 1))
            {
                if (terms.TryGetValue(row.TermId, out var text))
                    entries[text] = row.Text;
            }
        }

        return new LocaleDictionary(locale, VersionOf(entries), entries, timeProvider.GetUtcNow());
    }

    private static string VersionOf(Dictionary<string, string> entries)
    {
        var builder = new StringBuilder();
        foreach (var (key, value) in entries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            builder.Append(key).Append('\u0001').Append(value).Append('\u0002');
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())))[..16].ToLowerInvariant();
    }
}
