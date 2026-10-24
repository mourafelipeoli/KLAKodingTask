using System.Text.Json;
using System.Text.RegularExpressions;

namespace KodingTask.Api.Translations;

/// <summary>
/// The dictionaries that ship with the code: the web client's (built into the client too) and
/// the server's (email texts). They are imported as reviewed translations and are the
/// fallback when the database has nothing.
/// </summary>
public sealed partial class BakedDictionaries
{
    private const string InterfacePrefix = "KodingTask.Api.Translations.Baked.";
    private const string ServerPrefix = "KodingTask.Api.Messaging.Texts.";

    private readonly Dictionary<string, Dictionary<string, string>> _lookup;
    private readonly HashSet<string> _known;

    private BakedDictionaries(
        IReadOnlyDictionary<string, IReadOnlyList<KeyValuePair<string, string>>> interfaceTexts,
        IReadOnlyDictionary<string, IReadOnlyList<KeyValuePair<string, string>>> serverTexts)
    {
        Interface = interfaceTexts;
        Server = serverTexts;
        var byLocale = new Dictionary<string, IReadOnlyList<KeyValuePair<string, string>>>(StringComparer.OrdinalIgnoreCase);
        foreach (var locale in interfaceTexts.Keys.Union(serverTexts.Keys, StringComparer.OrdinalIgnoreCase))
        {
            var merged = new List<KeyValuePair<string, string>>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in interfaceTexts.GetValueOrDefault(locale, []).Concat(serverTexts.GetValueOrDefault(locale, [])))
            {
                if (seen.Add(entry.Key))
                    merged.Add(entry);
            }

            byLocale[locale] = merged;
        }

        ByLocale = byLocale;
        SourceKeys = byLocale.TryGetValue(TranslationLocales.Source, out var source) ? [.. source.Select(entry => entry.Key)] : [];
        _known = new HashSet<string>(SourceKeys, StringComparer.Ordinal);
        _lookup = byLocale.ToDictionary(pair => pair.Key, pair => pair.Value.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, IReadOnlyList<KeyValuePair<string, string>>> Interface { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<KeyValuePair<string, string>>> Server { get; }

    /// <summary>Both sets, the web client's first, in file order.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<KeyValuePair<string, string>>> ByLocale { get; }

    /// <summary>The English texts in the order of the English dictionary files.</summary>
    public IReadOnlyList<string> SourceKeys { get; }

    public bool Knows(string text) => _known.Contains(text);

    public string? Lookup(string locale, string text) =>
        _lookup.TryGetValue(locale, out var entries) && entries.TryGetValue(text, out var value) ? value : null;

    public static IReadOnlyList<string> PlaceholdersOf(string text) =>
        [.. Placeholder().Matches(text).Select(match => match.Groups[1].Value).Order(StringComparer.Ordinal)];

    public static BakedDictionaries Load() => new(Read(InterfacePrefix), Read(ServerPrefix));

    private static Dictionary<string, IReadOnlyList<KeyValuePair<string, string>>> Read(string prefix)
    {
        var assembly = typeof(BakedDictionaries).Assembly;
        var byLocale = new Dictionary<string, IReadOnlyList<KeyValuePair<string, string>>>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in assembly.GetManifestResourceNames().Where(name => name.StartsWith(prefix, StringComparison.Ordinal)))
        {
            var locale = Path.GetFileNameWithoutExtension(name[prefix.Length..]);
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var document = JsonDocument.Parse(stream);
            byLocale[locale] = [.. document.RootElement.EnumerateObject().Select(property => KeyValuePair.Create(property.Name, property.Value.GetString() ?? string.Empty))];
        }

        return byLocale;
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex Placeholder();
}
