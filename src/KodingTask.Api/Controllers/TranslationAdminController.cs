using System.Text.Encodings.Web;
using System.Text.Json;
using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Api.Features;
using KodingTask.Api.Hosting;
using KodingTask.Api.Translations;
using KodingTask.Data;
using KodingTask.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/admin/translations")]
[Produces("application/json")]
[Authorize(Policy = AdminOptions.Policy)]
[EnableRateLimiting(RequestRateLimits.Account)]
[ValidateAntiforgeryHeader]
public sealed class TranslationAdminController(
    KodingTaskDbContext database,
    BakedDictionaries baked,
    TranslationIndex index,
    TranslationBatchSignal signal,
    FeatureAvailability features,
    TimeProvider timeProvider) : ControllerBase
{
    private const int MaxTranslationLength = 2000;

    private static readonly JsonSerializerOptions ExportFormat = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminTranslationRow>>> List([FromQuery] string? locale, CancellationToken cancellationToken)
    {
        if (EditableLocale(locale) is not { } target)
            return NotFound(ApiErrors.UnknownLocale);

        var parent = TranslationLocales.Derived.GetValueOrDefault(target);
        var terms = await database.Terms.AsNoTracking().Where(term => term.IsActive).OrderBy(term => term.Id).ToListAsync(cancellationToken);
        var rows = await database.Translations.AsNoTracking()
            .Where(translation => translation.Locale == target || translation.Locale == parent)
            .ToListAsync(cancellationToken);
        var own = rows.Where(row => row.Locale == target).ToDictionary(row => row.TermId);
        var inherited = rows.Where(row => row.Locale == parent).ToDictionary(row => row.TermId, row => row.Text);

        return terms.Select(term => own.TryGetValue(term.Id, out var row)
                ? new AdminTranslationRow(term.Id, term.Text, term.Origin.ToString(), row.Text, inherited.GetValueOrDefault(term.Id), row.Source.ToString(), row.Reviewed, AsUtc(row.UpdatedAtUtc))
                : new AdminTranslationRow(term.Id, term.Text, term.Origin.ToString(), null, inherited.GetValueOrDefault(term.Id), null, false, null))
            .ToList();
    }

    [HttpPut]
    public async Task<ActionResult<AdminTranslationRow>> Update([FromBody] AdminTranslationUpdate request, CancellationToken cancellationToken)
    {
        if (EditableLocale(request.Locale) is not { } target)
            return NotFound(ApiErrors.UnknownLocale);
        var term = await database.Terms.FirstOrDefaultAsync(candidate => candidate.Id == request.TermId && candidate.IsActive, cancellationToken);
        if (term is null)
            return NotFound(ApiErrors.UnknownTerm);

        var text = request.Text?.Trim() ?? string.Empty;
        var isDerived = TranslationLocales.Derived.ContainsKey(target);
        if (text.Length > MaxTranslationLength || (text.Length == 0 && !isDerived))
            return BadRequest(ApiErrors.InvalidTranslation);

        var translation = await database.Translations.FirstOrDefaultAsync(row => row.TermId == term.Id && row.Locale == target, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (text.Length == 0)
        {
            if (translation is not null)
                database.Translations.Remove(translation);
            translation = null;
        }
        else if (translation is null)
        {
            translation = new Translation { TermId = term.Id, Locale = target, Text = text, Source = TranslationSource.Manual, Reviewed = request.Reviewed, UpdatedAtUtc = now };
            database.Translations.Add(translation);
        }
        else
        {
            if (translation.Text != text)
                translation.Source = TranslationSource.Manual;
            translation.Text = text;
            translation.Reviewed = request.Reviewed;
            translation.UpdatedAtUtc = now;
        }

        await database.SaveChangesAsync(cancellationToken);
        index.Invalidate();
        return translation is null
            ? new AdminTranslationRow(term.Id, term.Text, term.Origin.ToString(), null, null, null, false, null)
            : new AdminTranslationRow(term.Id, term.Text, term.Origin.ToString(), translation.Text, null, translation.Source.ToString(), translation.Reviewed, AsUtc(translation.UpdatedAtUtc));
    }

    [HttpPost("run")]
    public IActionResult Run()
    {
        if (!features.MachineTranslation)
            return StatusCode(ApiErrors.StatusOf(ApiErrors.MachineTranslationUnavailable), ApiErrors.MachineTranslationUnavailable);
        signal.Notify();
        return Accepted();
    }

    [HttpGet("export/{locale}")]
    public async Task<IActionResult> Export(string locale, CancellationToken cancellationToken)
    {
        if (TranslationLocales.Canonical(locale) is not { } target)
            return NotFound(ApiErrors.UnknownLocale);

        var terms = await database.Terms.AsNoTracking().Where(term => term.IsActive).ToListAsync(cancellationToken);
        var order = baked.SourceKeys.Select((key, position) => (key, position)).ToDictionary(pair => pair.key, pair => pair.position, StringComparer.Ordinal);
        var ordered = terms.OrderBy(term => order.TryGetValue(term.Text, out var position) ? position : int.MaxValue).ThenBy(term => term.Id).ToList();

        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        if (target == TranslationLocales.Source)
        {
            foreach (var term in ordered)
                entries[term.Text] = term.Text;
        }
        else
        {
            var texts = await database.Translations.AsNoTracking().Where(row => row.Locale == target)
                .ToDictionaryAsync(row => row.TermId, row => row.Text, cancellationToken);
            foreach (var term in ordered.Where(term => texts.ContainsKey(term.Id)))
                entries[term.Text] = texts[term.Id];
        }

        var json = JsonSerializer.Serialize(entries, ExportFormat) + "\n";
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"{target}.json");
    }

    private static string? EditableLocale(string? locale) =>
        TranslationLocales.Canonical(locale) is { } canonical && canonical != TranslationLocales.Source ? canonical : null;

    private static DateTimeOffset AsUtc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
