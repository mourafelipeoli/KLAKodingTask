namespace KodingTask.Api.Messaging;

public static class MetaLanguages
{
    private static readonly Dictionary<string, string> ByLocale = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en-US"] = "en_US",
        ["pt-BR"] = "pt_BR",
        ["de-DE"] = "de",
        ["fr-FR"] = "fr",
        ["fr-CA"] = "fr",
        ["fr-CH"] = "fr",
        ["fr-BE"] = "fr",
        ["it-IT"] = "it",
        ["es-ES"] = "es_ES",
        ["pl-PL"] = "pl",
        ["zh-CN"] = "zh_CN",
        ["zh-TW"] = "zh_TW",
        ["ja-JP"] = "ja",
        ["ko-KR"] = "ko",
    };

    public static string? For(string? locale) =>
        locale is not null && ByLocale.TryGetValue(locale, out var code) ? code : null;
}
