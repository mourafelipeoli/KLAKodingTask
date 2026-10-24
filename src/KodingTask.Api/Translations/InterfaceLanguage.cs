namespace KodingTask.Api.Translations;

/// <summary>
/// The interface languages. Adding a member is all it takes to offer a new language: the
/// language menu lists it, machine translation translates every text into it, and the
/// server answers in it.
/// </summary>
public enum InterfaceLanguage
{
    [Language("en-US", "English (US)")]
    EnglishUnitedStates,

    [Language("pt-BR", "Português (Brasil)")]
    PortugueseBrazil,

    [Language("de-DE", "Deutsch (Deutschland)")]
    GermanGermany,

    [Language("fr-FR", "Français (France)")]
    FrenchFrance,

    [Language("fr-CA", "Français (Canada)", VariantOf = "fr-FR")]
    FrenchCanada,

    [Language("fr-CH", "Français (Suisse)", VariantOf = "fr-FR")]
    FrenchSwitzerland,

    [Language("fr-BE", "Français (Belgique)", VariantOf = "fr-FR")]
    FrenchBelgium,

    [Language("it-IT", "Italiano (Italia)")]
    ItalianItaly,

    [Language("es-ES", "Español (España)")]
    SpanishSpain,

    [Language("pl-PL", "Polski (Polska)")]
    PolishPoland,

    [Language("zh-CN", "简体中文")]
    ChineseSimplified,

    [Language("zh-TW", "繁體中文", DeepL = "ZH-HANT")]
    ChineseTraditional,

    [Language("ja-JP", "日本語")]
    Japanese,

    [Language("ko-KR", "한국어")]
    Korean,
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class LanguageAttribute(string code, string nativeName) : Attribute
{
    public string Code { get; } = code;

    public string NativeName { get; } = nativeName;

    /// <summary>The DeepL target code, when the rule in <see cref="InterfaceLanguageInfo"/> does not give the right one.</summary>
    public string? DeepL { get; init; }

    /// <summary>A regional variant: it inherits this language's texts and only overrides what differs.</summary>
    public string? VariantOf { get; init; }
}

public sealed record InterfaceLanguageInfo(string Code, string NativeName, string? DeepL, string? VariantOf)
{
    public static InterfaceLanguageInfo Describe(InterfaceLanguage language)
    {
        var field = typeof(InterfaceLanguage).GetField(language.ToString())!;
        var attribute = (LanguageAttribute?)Attribute.GetCustomAttribute(field, typeof(LanguageAttribute))
            ?? throw new InvalidOperationException($"{language} has no [Language] attribute.");
        var deepL = attribute.VariantOf is null ? attribute.DeepL ?? DeepLTargetFor(attribute.Code) : null;
        return new InterfaceLanguageInfo(attribute.Code, attribute.NativeName, deepL, attribute.VariantOf);
    }

    // DeepL wants the variant for English and Portuguese (EN-US, PT-BR), simplified script by
    // default for Chinese, and the bare language for everything else (DE, FR, NL, SV, UK…).
    private static string DeepLTargetFor(string code)
    {
        var language = code.Split('-')[0].ToUpperInvariant();
        return language switch
        {
            "EN" or "PT" => code.ToUpperInvariant(),
            "ZH" => "ZH-HANS",
            _ => language,
        };
    }
}
