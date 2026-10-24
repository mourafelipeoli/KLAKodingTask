using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Translations;

public sealed class TranslationSeeder(KodingTaskDbContext database, BakedDictionaries baked, TimeProvider timeProvider, ILogger<TranslationSeeder> logger)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var terms = await database.Terms.ToDictionaryAsync(term => term.TextHash, cancellationToken);
        var sourceHashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in baked.SourceKeys)
        {
            var hash = TermText.Hash(key);
            sourceHashes.Add(hash);
            if (!terms.TryGetValue(hash, out var term))
            {
                term = new Term { Text = key, TextHash = hash, Origin = TermOrigin.Code, IsActive = true, CreatedAtUtc = now };
                database.Terms.Add(term);
                terms[hash] = term;
            }
            else
            {
                term.Origin = TermOrigin.Code;
                term.IsActive = true;
            }
        }

        foreach (var term in terms.Values.Where(term => term.Origin == TermOrigin.Code && !sourceHashes.Contains(term.TextHash)))
            term.IsActive = false;
        var changes = await database.SaveChangesAsync(cancellationToken);

        var existing = await database.Translations.ToDictionaryAsync(translation => (translation.TermId, translation.Locale), cancellationToken);
        foreach (var (locale, entries) in baked.ByLocale.Where(pair => !string.Equals(pair.Key, TranslationLocales.Source, StringComparison.OrdinalIgnoreCase)))
        {
            var bakedTermIds = new HashSet<int>();
            foreach (var (key, text) in entries)
            {
                if (!terms.TryGetValue(TermText.Hash(key), out var term))
                    continue;
                bakedTermIds.Add(term.Id);
                if (!existing.TryGetValue((term.Id, locale), out var translation))
                {
                    database.Translations.Add(new Translation
                    {
                        TermId = term.Id,
                        Locale = locale,
                        Text = text,
                        Source = TranslationSource.Baked,
                        Reviewed = true,
                        UpdatedAtUtc = now,
                    });
                }
                else if (translation.Source != TranslationSource.Manual && (translation.Source != TranslationSource.Baked || translation.Text != text))
                {
                    translation.Text = text;
                    translation.Source = TranslationSource.Baked;
                    translation.Reviewed = true;
                    translation.UpdatedAtUtc = now;
                }
            }

            database.Translations.RemoveRange(existing.Values.Where(translation =>
                translation.Locale == locale && translation.Source == TranslationSource.Baked && !bakedTermIds.Contains(translation.TermId)));
        }

        changes += await database.SaveChangesAsync(cancellationToken);
        if (changes > 0)
            logger.LogInformation("[Translations] Imported the bundled dictionaries: {Changes} rows changed.", changes);
        return changes;
    }
}
