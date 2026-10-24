using KodingTask.Api.Limits;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Translations;

public sealed class TranslationBatch(
    KodingTaskDbContext database,
    IMachineTranslator translator,
    IOptions<DeepLOptions> options,
    UsageLimitsSource limitsSource,
    UsageLimiter limiter,
    TranslationIndex index,
    TimeProvider timeProvider,
    ILogger<TranslationBatch> logger)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var limits = await limitsSource.GetAsync(cancellationToken);
        if (limits.KillSwitch)
            return 0;

        var batchSize = Math.Clamp(options.Value.BatchSize, 1, 50);
        var translated = 0;
        try
        {
            foreach (var locale in TranslationLocales.MachineTargets.Keys)
            {
                while (true)
                {
                    var pending = await database.Terms
                        .Where(term => term.IsActive && !term.Translations.Any(translation => translation.Locale == locale))
                        .OrderBy(term => term.Id)
                        .Take(batchSize)
                        .Select(term => new { term.Id, term.Text })
                        .ToListAsync(cancellationToken);
                    if (pending.Count == 0)
                        break;

                    var characters = pending.Sum(term => term.Text.Length);
                    if (!await limiter.TryConsumeAsync(Counters.TranslationCharacters, limits.TranslationCharactersPerDay, characters, cancellationToken))
                    {
                        logger.LogWarning("[Translations] Today's machine translation allowance is used up; {Pending} texts wait for tomorrow.", pending.Count);
                        return translated;
                    }

                    var texts = await translator.TranslateAsync([.. pending.Select(term => term.Text)], locale, cancellationToken);
                    var now = timeProvider.GetUtcNow().UtcDateTime;
                    for (var position = 0; position < pending.Count; position++)
                    {
                        database.Translations.Add(new Translation
                        {
                            TermId = pending[position].Id,
                            Locale = locale,
                            Text = texts[position],
                            Source = TranslationSource.Machine,
                            Reviewed = false,
                            UpdatedAtUtc = now,
                        });
                    }

                    await database.SaveChangesAsync(cancellationToken);
                    translated += pending.Count;
                }
            }
        }
        catch (MachineTranslationException exception)
        {
            logger.LogWarning("[Translations] Machine translation stopped: {Reason}", exception.Message);
        }
        finally
        {
            if (translated > 0)
            {
                index.Invalidate();
                logger.LogInformation("[Translations] {Count} texts translated by machine.", translated);
            }
        }

        return translated;
    }
}
