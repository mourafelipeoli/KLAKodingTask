using System.Diagnostics.CodeAnalysis;
using KodingTask.Conversion.Languages;

namespace KodingTask.Conversion;

public sealed class SpellerCatalog
{
    private readonly Dictionary<string, ICurrencySpeller> _byCode;

    public SpellerCatalog(IEnumerable<ICurrencySpeller> spellers)
    {
        var ordered = spellers.ToList();
        _byCode = ordered.ToDictionary(speller => speller.Language.Code, StringComparer.OrdinalIgnoreCase);
        Languages = ordered.Select(speller => speller.Language).ToList();
    }

    public static SpellerCatalog Default { get; } = new(
    [
        new EnglishSpeller(),
        new PortugueseSpeller(),
        new GermanSpeller(),
        new FrenchSpeller(new("fr-FR", "French (France)", "Français (France)"), FrenchNumbering.Standard),
        new FrenchSpeller(new("fr-CA", "French (Canada)", "Français (Canada)"), FrenchNumbering.Standard),
        new FrenchSpeller(new("fr-CH", "French (Switzerland)", "Français (Suisse)"), FrenchNumbering.Swiss),
        new FrenchSpeller(new("fr-BE", "French (Belgium)", "Français (Belgique)"), FrenchNumbering.Belgian),
        new ItalianSpeller(),
        new ChineseSpeller(new("zh-CN", "Chinese (Simplified, Mainland China)", "简体中文"), ChineseScript.Simplified),
        new ChineseSpeller(new("zh-TW", "Chinese (Traditional, Taiwan)", "繁體中文"), ChineseScript.Traditional),
        new JapaneseSpeller(),
        new KoreanSpeller(),
    ]);

    public IReadOnlyList<LanguageInfo> Languages { get; }

    public bool TryGet(string? languageCode, [NotNullWhen(true)] out ICurrencySpeller? speller)
    {
        speller = null;
        return languageCode is not null && _byCode.TryGetValue(languageCode.Replace('_', '-'), out speller);
    }
}
