namespace KodingTask.Api.Translations;

/// <summary>The interface languages as data, read once from <see cref="InterfaceLanguage"/>.</summary>
public static class TranslationLocales
{
    public const string Source = "en-US";

    public static IReadOnlyList<InterfaceLanguageInfo> Languages { get; } =
        [.. Enum.GetValues<InterfaceLanguage>().Select(InterfaceLanguageInfo.Describe)];

    public static IReadOnlyList<string> All { get; } = [.. Languages.Select(language => language.Code)];

    /// <summary>Languages machine translation translates into, with their DeepL target code.</summary>
    public static IReadOnlyDictionary<string, string> MachineTargets { get; } = Languages
        .Where(language => language.Code != Source && language.DeepL is not null)
        .ToDictionary(language => language.Code, language => language.DeepL!, StringComparer.OrdinalIgnoreCase);

    /// <summary>Regional variants and the language they inherit from.</summary>
    public static IReadOnlyDictionary<string, string> Derived { get; } = Languages
        .Where(language => language.VariantOf is not null)
        .ToDictionary(language => language.Code, language => language.VariantOf!, StringComparer.OrdinalIgnoreCase);

    public static string? Canonical(string? locale) =>
        All.FirstOrDefault(candidate => string.Equals(candidate, locale?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string? Match(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return null;
        var exact = Canonical(tag);
        if (exact is not null)
            return exact;

        var normalized = tag.Trim().Replace('_', '-').ToLowerInvariant();
        var language = normalized.Split('-')[0];
        if (language == "zh" && All.Contains("zh-TW") && All.Contains("zh-CN"))
            return normalized.Contains("-tw") || normalized.Contains("-hk") || normalized.Contains("-mo") || normalized.Contains("hant") ? "zh-TW" : "zh-CN";
        return All.FirstOrDefault(candidate => candidate.StartsWith(language + "-", StringComparison.OrdinalIgnoreCase));
    }
}
