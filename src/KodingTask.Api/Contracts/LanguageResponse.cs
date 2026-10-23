using KodingTask.Conversion;

namespace KodingTask.Api.Contracts;

public sealed record LanguageResponse(string Code, string EnglishName, string NativeName)
{
    public static LanguageResponse From(LanguageInfo language) =>
        new(language.Code, language.EnglishName, language.NativeName);
}
