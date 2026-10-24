using System.Text.RegularExpressions;

namespace KodingTask.Api.Translations;

/// <summary>
/// Translates what the server itself writes (emails, error messages) with the same terms as
/// the web client: the database dictionary, then the bundled one, then the English text. A
/// text it does not know is queued and becomes a term on the next batch.
/// </summary>
public sealed partial class ServerTexts(TranslationIndex index, BakedDictionaries baked, PendingTerms pending)
{
    public string Translate(string text, string? locale, IReadOnlyDictionary<string, string>? parameters = null)
    {
        var language = TranslationLocales.Canonical(locale) ?? TranslationLocales.Source;
        if (!baked.Knows(text) && !index.KnowsTerm(text))
            pending.Add(text);

        var translated = language == TranslationLocales.Source ? text : Lookup(language, text) ?? text;
        return parameters is null
            ? translated
            : Placeholder().Replace(translated, match => parameters.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);
    }

    private string? Lookup(string language, string text)
    {
        if (index.Cached(language)?.Entries.TryGetValue(text, out var stored) == true)
            return stored;
        if (baked.Lookup(language, text) is { } bundled)
            return bundled;
        return TranslationLocales.Derived.TryGetValue(language, out var parent) ? baked.Lookup(parent, text) : null;
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();
}
