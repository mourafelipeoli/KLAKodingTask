using System.Text.Json;
using System.Text.RegularExpressions;

namespace KodingTask.Api.Messaging;

public sealed partial class MessageTexts
{
    public const string SourceLocale = "en-US";

    private const string ResourcePrefix = "KodingTask.Api.Messaging.Texts.";

    private static readonly Dictionary<string, string> RegionalParents = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fr-CA"] = "fr-FR",
        ["fr-CH"] = "fr-FR",
        ["fr-BE"] = "fr-FR",
    };

    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _byLocale;

    public MessageTexts()
        : this(LoadEmbedded())
    {
    }

    internal MessageTexts(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> byLocale) => _byLocale = byLocale;

    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Dictionaries => _byLocale;

    public string Translate(string? locale, string key, IReadOnlyDictionary<string, string>? parameters = null)
    {
        var text = Lookup(locale, key) ?? key;
        return parameters is null
            ? text
            : Placeholder().Replace(text, match => parameters.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);
    }

    public static IReadOnlyList<string> PlaceholdersOf(string text) =>
        [.. Placeholder().Matches(text).Select(match => match.Groups[1].Value).Order(StringComparer.Ordinal)];

    private string? Lookup(string? locale, string key)
    {
        if (locale is null)
            return null;
        if (_byLocale.TryGetValue(locale, out var own) && own.TryGetValue(key, out var text))
            return text;
        return RegionalParents.TryGetValue(locale, out var parent)
            && _byLocale.TryGetValue(parent, out var inherited)
            && inherited.TryGetValue(key, out var fallback)
            ? fallback
            : null;
    }

    private static Dictionary<string, IReadOnlyDictionary<string, string>> LoadEmbedded()
    {
        var assembly = typeof(MessageTexts).Assembly;
        var dictionaries = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in assembly.GetManifestResourceNames().Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            var locale = name[ResourcePrefix.Length..^".json".Length];
            dictionaries[locale] = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
        }

        return dictionaries;
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();
}
